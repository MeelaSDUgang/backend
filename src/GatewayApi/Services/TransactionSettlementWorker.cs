using System.Text;
using System.Text.Json;
using GatewayApi.Data;
using GatewayApi.Entities;
using GatewayApi.Enums;
using GatewayApi.Models;
using GatewayApi.Services.Banking;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Services;

public class TransactionSettlementWorker : BackgroundService
{
    private static readonly Random Rng = new();
    private readonly IEnumerable<IPaymentAdapter> _adapters;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TransactionSettlementWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public TransactionSettlementWorker(
        IServiceScopeFactory scopeFactory,
        IEnumerable<IPaymentAdapter> adapters,
        IHttpClientFactory httpClientFactory,
        ILogger<TransactionSettlementWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _adapters = adapters;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TransactionSettlementWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingTransactions(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in settlement worker");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessPendingTransactions(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var settlementThreshold = DateTime.UtcNow.AddSeconds(-Rng.Next(30, 60));

        var pendingTransactions = await db.Transactions
            .Include(t => t.Merchant)
            .Include(t => t.BankAdapter)
            .Where(t => t.TransactionStatus == TransactionStatus.Pending
                        && t.UpdatedAt <= settlementThreshold)
            .ToListAsync(ct);

        foreach (var tx in pendingTransactions)
            try
            {
                await SettleTransaction(db, tx, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to settle Tx {TxId}", tx.Id);
            }
    }

    private async Task SettleTransaction(AppDbContext db, Transaction tx, CancellationToken ct)
    {
        var adapter = _adapters.FirstOrDefault(a => a.RoutingKey == tx.BankAdapter.RoutingKey);

        if (adapter is null)
        {
            tx.TransactionStatus = TransactionStatus.Failed;
            tx.FailureReason = $"No adapter registered for bank '{tx.BankAdapter.Name}'";
            tx.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return;
        }

        var context = new PaymentContext(
            tx.Id, tx.GatewayType, tx.BankId,
            tx.Amount, tx.Currency, tx.RawPayload);

        BankResponse bankResult;
        try
        {
            bankResult = await adapter.ProcessPaymentAsync(context, ct);
        }
        catch (Exception ex)
        {
            tx.TransactionStatus = TransactionStatus.Failed;
            tx.FailureReason = $"Adapter error: {ex.Message}";
            tx.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            _logger.LogError(ex, "Adapter threw for Tx {TxId}", tx.Id);
            return;
        }

        tx.TransactionStatus = bankResult.Success
            ? TransactionStatus.Completed
            : TransactionStatus.Failed;

        tx.BankReferenceId = bankResult.Success ? bankResult.BankReferenceId : null;
        tx.FailureReason = bankResult.FailureReason;
        tx.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Tx {TxId} settled → {Status} (ref: {BankRef})",
            tx.Id, tx.TransactionStatus, tx.BankReferenceId);

        if (!string.IsNullOrWhiteSpace(tx.Merchant.WebhookUrl)) await SendWebhookAsync(tx, ct);
    }

    private async Task SendWebhookAsync(Transaction tx, CancellationToken ct)
    {
        var payload = new TransactionStatusResponse(
            tx.Id.ToString(),
            tx.BankId.ToString(),
            tx.GatewayType.ToString(),
            tx.TransactionStatus.ToString().ToUpper(),
            tx.Amount,
            tx.Currency,
            tx.BankReferenceId,
            tx.FailureReason,
            tx.CreatedAt,
            tx.UpdatedAt);

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        try
        {
            var client = _httpClientFactory.CreateClient("Webhook");
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(tx.Merchant.WebhookUrl, content, ct);

            _logger.LogInformation(
                "Webhook sent for Tx {TxId} to {Url} — {StatusCode}",
                tx.Id, tx.Merchant.WebhookUrl, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Webhook failed for Tx {TxId} to {Url}",
                tx.Id, tx.Merchant.WebhookUrl);
        }
    }
}