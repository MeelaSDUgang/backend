using GatewayApi.Data;
using GatewayApi.Entities;
using GatewayApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Services.Banking;

public class PaymentOrchestrator
{
    private readonly IEnumerable<IPaymentAdapter> _adapters;
    private readonly AppDbContext _db;
    private readonly ILogger<PaymentOrchestrator> _logger;

    public PaymentOrchestrator(
        IEnumerable<IPaymentAdapter> adapters,
        AppDbContext db,
        ILogger<PaymentOrchestrator> logger)
    {
        _adapters = adapters;
        _db = db;
        _logger = logger;
    }

    public async Task<Transaction> ProcessAsync(
        Transaction transaction,
        CancellationToken ct = default)
    {
        var bank = await _db.BankAdapters
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == transaction.BankId, ct);

        if (bank is null)
        {
            transaction.TransactionStatus = TransactionStatus.Failed;
            transaction.FailureReason = "Bank not found";
            transaction.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return transaction;
        }

        if (!bank.IsActive)
        {
            transaction.TransactionStatus = TransactionStatus.Failed;
            transaction.FailureReason = $"Bank '{bank.Name}' is currently offline";
            transaction.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return transaction;
        }

        var supportedTypes = bank.SupportedGatewayTypes
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim());

        if (!supportedTypes.Contains(transaction.GatewayType.ToString()))
        {
            transaction.TransactionStatus = TransactionStatus.Failed;
            transaction.FailureReason = $"Bank '{bank.Name}' does not support {transaction.GatewayType} payments";
            transaction.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return transaction;
        }

        var adapter = _adapters.FirstOrDefault(a => a.RoutingKey == bank.RoutingKey);

        if (adapter is null)
        {
            transaction.TransactionStatus = TransactionStatus.Failed;
            transaction.FailureReason = $"No adapter registered for bank '{bank.Name}'";
            transaction.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return transaction;
        }

        transaction.TransactionStatus = TransactionStatus.Pending;
        transaction.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Tx {TxId} → PENDING, routed to [{AdapterName}] — {Type} {Amount} {Currency}",
            transaction.Id, adapter.RoutingKey, transaction.GatewayType,
            transaction.Amount, transaction.Currency);

        return transaction;
    }
}