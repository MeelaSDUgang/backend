using System.Text.Json;
using GatewayApi.Data;
using GatewayApi.Entities;
using GatewayApi.Enums;
using GatewayApi.Filters;
using GatewayApi.Models;
using GatewayApi.Services;
using GatewayApi.Services.Banking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[ServiceFilter(typeof(HmacAuthFilter))]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IdempotencyService _idempotency;
    private readonly ILogger<PaymentsController> _logger;
    private readonly PaymentOrchestrator _orchestrator;

    public PaymentsController(
        AppDbContext db,
        PaymentOrchestrator orchestrator,
        IdempotencyService idempotency,
        ILogger<PaymentsController> logger)
    {
        _db = db;
        _orchestrator = orchestrator;
        _idempotency = idempotency;
        _logger = logger;
    }

    [HttpPost("p2p")]
    public async Task<IActionResult> ProcessP2P([FromBody] P2PRequest request, CancellationToken ct)
    {
        return await ProcessPayment(request.BankId, request.Amount, request.Currency, GatewayType.P2P, request, ct);
    }

    [HttpGet("{transactionId:guid}")]
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
            tx.GatewayType.ToString(),
            tx.TransactionStatus.ToString().ToUpper(),
            tx.Amount,
            tx.Currency,
            tx.BankReferenceId,
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

        if (!Guid.TryParse(bankIdStr, out var bankId))
            return BadRequest(new ErrorResponse("Invalid BankId format", "Must be a valid UUID"));

        var bankExists = await _db.BankAdapters.AnyAsync(b => b.Id == bankId, ct);
        if (!bankExists)
            return BadRequest(new ErrorResponse("Bank not found", $"No bank adapter with ID '{bankId}'"));

        if (amount <= 0)
            return BadRequest(new ErrorResponse("Amount must be greater than zero"));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            return BadRequest(new ErrorResponse("Currency must be a 3-letter ISO code"));

        var rawPayload = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var riskScore = 0;
        var reason = "hui";

        var fraudResult = false;

        if (fraudResult)
        {
            _logger.LogWarning(
                "FRAUD BLOCKED — {Type} {Amount} {Currency} for {Merchant}: score={Score}, reason={Reason}",
                gatewayType, amount, currency, user.FullName,
                riskScore, reason);

            var rejectedTx = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                BankId = bankId,
                IdempotencyKey = idempotencyKey,
                Account = ExtractAccountIdentifier(request),
                Amount = amount,
                Currency = currency.ToUpperInvariant(),
                GatewayType = gatewayType,
                TransactionStatus = TransactionStatus.Rejected,
                AiRiskScore = riskScore,
                AiRiskReason = reason,
                RawPayload = rawPayload,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Transactions.Add(rejectedTx);
            await _db.SaveChangesAsync(ct);

            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                error = "Transaction rejected by Anti-Fraud system",
                transactionId = rejectedTx.Id,
                status = "REJECTED",
                fraudScore = riskScore / 100.0,
                reason
            });
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            BankId = bankId,
            IdempotencyKey = idempotencyKey,
            Account = ExtractAccountIdentifier(request),
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            GatewayType = gatewayType,
            TransactionStatus = TransactionStatus.Created,
            AiRiskScore = riskScore,
            AiRiskReason = reason,
            RawPayload = rawPayload,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Tx {TxId} CREATED — {Type} for {MerchantName} ({Amount} {Currency}), fraudScore={FraudScore}",
            transaction.Id, gatewayType, user.FullName, amount, currency, riskScore);

        var updatedTx = await _orchestrator.ProcessAsync(transaction, ct);

        var response = new PaymentResponse(
            updatedTx.Id.ToString(),
            updatedTx.BankId.ToString(),
            updatedTx.GatewayType.ToString(),
            updatedTx.TransactionStatus.ToString().ToUpper(),
            updatedTx.Amount,
            updatedTx.Currency,
            updatedTx.BankReferenceId,
            updatedTx.UpdatedAt);

        Response.Headers["X-Correlation-Id"] = updatedTx.Id.ToString();

        return StatusCode(StatusCodes.Status201Created, response);
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
}