using System.Text.Json;
using GatewayApi.Data;
using GatewayApi.Entities;
using GatewayApi.Enums;
using GatewayApi.Filters;
using GatewayApi.Models;
using GatewayApi.Services;
using GatewayApi.Services.Banking;
using GatewayApi.Services.Fraud;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[ServiceFilter(typeof(HmacAuthFilter))]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IFraudEvaluationService _fraudEvaluationService;
    private readonly IdempotencyService _idempotency;
    private readonly ILogger<PaymentsController> _logger;
    private readonly PaymentOrchestrator _orchestrator;

    public PaymentsController(
        AppDbContext db,
        PaymentOrchestrator orchestrator,
        IdempotencyService idempotency,
        IFraudEvaluationService fraudEvaluationService,
        ILogger<PaymentsController> logger)
    {
        _db = db;
        _orchestrator = orchestrator;
        _idempotency = idempotency;
        _fraudEvaluationService = fraudEvaluationService;
        _logger = logger;
    }

    /// <summary>
    ///     Processes a P2P payment through fraud scoring and the bank adapter.
    /// </summary>
    /// <remarks>
    ///     Requires HMAC headers and a UUID Idempotency-Key header.
    ///     Fraud outcomes:
    ///     - none: payment is sent to bank and completed
    ///     - low: transaction is saved as PENDING
    ///     - medium: transaction is saved as REJECTED and sent to compliance review
    ///     - high: transaction is FAILED and the user account is blocked
    /// </remarks>
    [HttpPost("p2p")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ProcessP2P(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromBody] P2PRequest request,
        CancellationToken ct)
    {
        return await ProcessPayment(request.BankId, request.Amount, request.Currency, GatewayType.P2P, request, ct);
    }

    /// <summary>
    ///     Returns payment transaction status for the authenticated user.
    /// </summary>
    [HttpGet("{transactionId:guid}")]
    [ProducesResponseType(typeof(TransactionStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTransactionStatus(Guid transactionId, CancellationToken ct)
    {
        var getUser = GetUser();

        var tx = await _db.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == getUser.Id, ct);

        if (tx is null)
            return NotFound(new ErrorResponse("Transaction not found"));

        Response.Headers["X-Correlation-Id"] = tx.Id.ToString();

        return Ok(new TransactionStatusResponse(
            tx.Id.ToString(),
            tx.BankId.ToString(),
            tx.Type,
            tx.TransactionStatus.ToString().ToUpper(),
            tx.Amount,
            tx.NameDest,
            tx.NameOrig,
            tx.NewbalanceDest,
            tx.NewbalanceOrig,
            tx.OldbalanceDest,
            tx.OldbalanceOrg,
            tx.Step,
            tx.Label,
            tx.Currency,
            tx.FailureReason,
            tx.CreatedAt,
            tx.UpdatedAt));
    }

    private async Task<IActionResult> ProcessPayment<TRequest>(
        string bankIdStr, decimal amount, string currency,
        GatewayType gatewayType, TRequest request, CancellationToken ct)
    {
        var user = GetUser();

        if (!Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyHeader) ||
            !Guid.TryParse(idempotencyHeader, out var idempotencyKey))
            return BadRequest(new ErrorResponse(
                "Missing or invalid Idempotency-Key header",
                "Must be a valid UUID"));

        var cached = await _idempotency.CheckAsync(idempotencyKey, user.Id, ct);
        if (cached is not null)
        {
            Response.Headers["X-Correlation-Id"] = cached.TransactionId;
            Response.Headers["X-Idempotency-Hit"] = "true";
            return Ok(cached);
        }

        if (user.AccountStatus == AccountStatus.BLOCKED)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ErrorResponse(
                    "Account is blocked",
                    "Payments are not allowed for blocked accounts."));

        if (!Guid.TryParse(bankIdStr, out var bankId))
            return BadRequest(new ErrorResponse("Invalid BankId format", "Must be a valid UUID"));

        var bankExists = await _db.BankAdapters.AnyAsync(b => b.Id == bankId, ct);
        if (!bankExists)
            return BadRequest(new ErrorResponse("Bank not found", $"No bank adapter with ID '{bankId}'"));

        if (amount <= 0)
            return BadRequest(new ErrorResponse("Amount must be greater than zero"));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            return BadRequest(new ErrorResponse("Currency must be a 3-letter ISO code"));

        var transactionFields = ExtractTransactionFields(request, gatewayType);

        var fraudEvaluation = await _fraudEvaluationService.EvaluateAsync(
            new FraudScoringInput(
                transactionFields.Step,
                transactionFields.Type,
                amount,
                transactionFields.OldbalanceOrg,
                transactionFields.NewbalanceOrig,
                transactionFields.OldbalanceDest,
                transactionFields.NewbalanceDest,
                transactionFields.NameOrig,
                transactionFields.NameDest),
            ct);

        if (!fraudEvaluation.IsAvailable)
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ErrorResponse(
                    "Fraud scoring unavailable",
                    fraudEvaluation.Error));

        var transaction = CreateTransaction(
            user,
            bankId,
            idempotencyKey,
            ExtractAccountIdentifier(request),
            amount,
            currency,
            gatewayType,
            transactionFields);

        switch (fraudEvaluation.RiskLevel)
        {
            case FraudRiskLevel.Low:
                transaction.TransactionStatus = TransactionStatus.Pending;
                _db.Transactions.Add(transaction);
                await _db.SaveChangesAsync(ct);
                Response.Headers["X-Correlation-Id"] = transaction.Id.ToString();
                return StatusCode(StatusCodes.Status201Created, ToPaymentResponse(transaction));

            case FraudRiskLevel.Medium:
                transaction.TransactionStatus = TransactionStatus.Rejected;
                transaction.FailureReason = fraudEvaluation.Interpretation?.Summary ?? fraudEvaluation.Score!.RiskTier;
                _db.Transactions.Add(transaction);
                if (fraudEvaluation.Interpretation is not null)
                    _db.FraudReviews.Add(CreateFraudReview(transaction, fraudEvaluation));

                await _db.SaveChangesAsync(ct);
                Response.Headers["X-Correlation-Id"] = transaction.Id.ToString();
                return StatusCode(StatusCodes.Status202Accepted,
                    CreateFraudReviewResponse(transaction, fraudEvaluation));

            case FraudRiskLevel.High:
                transaction.TransactionStatus = TransactionStatus.Failed;
                transaction.FailureReason = fraudEvaluation.Interpretation?.Summary ?? fraudEvaluation.Score!.RiskTier;
                _db.Users.Attach(user);
                user.AccountStatus = AccountStatus.BLOCKED;
                user.UpdatedAt = DateTimeOffset.UtcNow;
                _db.Entry(user).Property(item => item.AccountStatus).IsModified = true;
                _db.Entry(user).Property(item => item.UpdatedAt).IsModified = true;
                _db.Transactions.Add(transaction);
                if (fraudEvaluation.Interpretation is not null)
                    _db.FraudReviews.Add(CreateFraudReview(transaction, fraudEvaluation));

                await _db.SaveChangesAsync(ct);
                Response.Headers["X-Correlation-Id"] = transaction.Id.ToString();
                return StatusCode(StatusCodes.Status403Forbidden,
                    CreateFraudReviewResponse(transaction, fraudEvaluation));
        }

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync(ct);

        var updatedTx = await _orchestrator.ProcessAsync(transaction, ct);

        Response.Headers["X-Correlation-Id"] = updatedTx.Id.ToString();

        return StatusCode(StatusCodes.Status201Created, ToPaymentResponse(updatedTx));
    }

    private User GetUser()
    {
        return (HttpContext.Items["User"] as User)!;
    }

    private static string ExtractAccountIdentifier<TRequest>(TRequest request)
    {
        return request switch
        {
            A2ARequest a2a => a2a.DebtorAccount.Iban,
            B2BRequest b2b => b2b.PayerInfo.AccountNumber,
            PayoutRequest b2c => b2c.FundingAccount.Iban,
            P2PRequest p2p => p2p.Sender.PhoneNumber,
            _ => "unknown"
        };
    }

    private static TransactionFields ExtractTransactionFields<TRequest>(TRequest request, GatewayType gatewayType)
    {
        return request switch
        {
            P2PRequest p2p => new TransactionFields(
                ValueOrDefault(p2p.NameDest, p2p.Receiver.PhoneNumber),
                ValueOrDefault(p2p.NameOrig, p2p.Sender.PhoneNumber),
                p2p.NewbalanceDest,
                p2p.NewbalanceOrig,
                p2p.OldbalanceDest,
                p2p.OldbalanceOrg,
                p2p.Step,
                ValueOrDefault(p2p.Type, "PAYMENT"),
                ValueOrDefault(p2p.Label, "Legitimate PAYMENT")),
            _ => new TransactionFields(
                ExtractAccountIdentifier(request),
                "unknown",
                0,
                0,
                0,
                0,
                0,
                gatewayType.ToString(),
                $"Legitimate {gatewayType}")
        };
    }

    private static string ValueOrDefault(string? value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
    }

    private static Transaction CreateTransaction(
        User user,
        Guid bankId,
        Guid idempotencyKey,
        string account,
        decimal amount,
        string currency,
        GatewayType gatewayType,
        TransactionFields transactionFields)
    {
        var now = DateTime.UtcNow;

        return new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            BankId = bankId,
            IdempotencyKey = idempotencyKey,
            Account = account,
            Amount = amount,
            NameDest = transactionFields.NameDest,
            NameOrig = transactionFields.NameOrig,
            NewbalanceDest = transactionFields.NewbalanceDest,
            NewbalanceOrig = transactionFields.NewbalanceOrig,
            OldbalanceDest = transactionFields.OldbalanceDest,
            OldbalanceOrg = transactionFields.OldbalanceOrg,
            Step = transactionFields.Step,
            Type = transactionFields.Type,
            Label = transactionFields.Label,
            Currency = currency.ToUpperInvariant(),
            GatewayType = gatewayType,
            TransactionStatus = TransactionStatus.Created,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static PaymentResponse ToPaymentResponse(Transaction transaction)
    {
        return new PaymentResponse(
            transaction.Id.ToString(),
            transaction.BankId.ToString(),
            transaction.Type,
            transaction.TransactionStatus.ToString().ToUpper(),
            transaction.Amount,
            transaction.NameDest,
            transaction.NameOrig,
            transaction.NewbalanceDest,
            transaction.NewbalanceOrig,
            transaction.OldbalanceDest,
            transaction.OldbalanceOrg,
            transaction.Step,
            transaction.Label,
            transaction.Currency,
            transaction.UpdatedAt);
    }

    private static object CreateFraudReviewResponse(Transaction transaction, FraudEvaluationResult fraudEvaluation)
    {
        return new
        {
            error = transaction.TransactionStatus == TransactionStatus.Failed
                ? "Transaction failed by Anti-Fraud system"
                : "Transaction sent to Compliance review",
            transactionId = transaction.Id,
            status = transaction.TransactionStatus.ToString().ToUpper(),
            fraudScore = fraudEvaluation.Score!.FraudScore,
            riskTier = fraudEvaluation.Interpretation?.RiskTier ?? fraudEvaluation.Score.RiskTier,
            summary = fraudEvaluation.Interpretation?.Summary,
            reasons = fraudEvaluation.Interpretation?.Reasons ?? [],
            advice = fraudEvaluation.Interpretation?.Advice
        };
    }

    private static FraudReview CreateFraudReview(Transaction transaction, FraudEvaluationResult fraudEvaluation)
    {
        var score = fraudEvaluation.Score!;
        var interpretation = fraudEvaluation.Interpretation!;

        return new FraudReview
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            FraudScore = score.FraudScore,
            RiskTier = ValueOrDefault(interpretation.RiskTier, score.RiskTier),
            IsFraud = score.IsFraud,
            Status = interpretation.Status,
            Summary = interpretation.Summary,
            ReasonsJson = JsonSerializer.Serialize(interpretation.Reasons),
            Advice = interpretation.Advice,
            EvaluatedAt = score.EvaluatedAt,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private sealed record TransactionFields(
        string NameDest,
        string NameOrig,
        decimal NewbalanceDest,
        decimal NewbalanceOrig,
        decimal OldbalanceDest,
        decimal OldbalanceOrg,
        int Step,
        string Type,
        string Label);
}