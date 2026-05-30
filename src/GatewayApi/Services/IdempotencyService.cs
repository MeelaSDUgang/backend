using GatewayApi.Data;
using GatewayApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Services;

public class IdempotencyService
{
    private readonly AppDbContext _db;
    private readonly ILogger<IdempotencyService> _logger;

    public IdempotencyService(AppDbContext db, ILogger<IdempotencyService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaymentResponse?> CheckAsync(Guid idempotencyKey, Guid merchantId, CancellationToken ct = default)
    {
        var existing = await _db.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                    t.IdempotencyKey == idempotencyKey &&
                    t.MerchantId == merchantId,
                ct);

        if (existing is null)
            return null;

        if ((DateTime.UtcNow - existing.CreatedAt).TotalHours > 24)
            return null;

        _logger.LogInformation("Idempotency hit for Tx {TxId} (key: {Key})", existing.Id, idempotencyKey);

        return new PaymentResponse(
            existing.Id.ToString(),
            existing.BankId.ToString(),
            existing.GatewayType.ToString(),
            existing.TransactionStatus.ToString().ToUpper(),
            existing.Amount,
            existing.Currency,
            existing.BankReferenceId,
            existing.UpdatedAt);
    }
}