using ComplianceDashboard.Data;
using ComplianceDashboard.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceDashboard.Services.TransactionProcessing;

public class TransactionReviewWorker(
    ITransactionWorkerQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<TransactionReviewWorker> logger) : BackgroundService
{
    private const string AccountStatusBlocked = "BLOCKED";
    private const string StatusCompleted = "Completed";
    private const string StatusFailed = "Failed";
    private const string StatusPending = "Pending";
    private const string StatusRejected = "Rejected";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var item = await queue.DequeueAsync(stoppingToken);
            await ProcessAsync(item, stoppingToken);
        }
    }

    private async Task ProcessAsync(TransactionWorkItem item, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DashboardDbContext>();

            var transaction = await dbContext.Transactions
                .Include(tx => tx.User)
                .FirstOrDefaultAsync(tx => tx.Id == item.TransactionId, cancellationToken);

            if (transaction is null)
            {
                logger.LogWarning(
                    "Transaction worker skipped missing transaction {TransactionId} from {Source}.",
                    item.TransactionId,
                    item.Source);
                return;
            }

            if (IsBlockedAccount(transaction.User.AccountStatus))
            {
                logger.LogInformation(
                    "Transaction worker skipped {TransactionId}: account is blocked.",
                    transaction.Id);
                return;
            }

            if (IsFinalStatus(transaction.TransactionStatus))
            {
                logger.LogInformation(
                    "Transaction worker skipped {TransactionId}: status {Status} is final.",
                    transaction.Id,
                    transaction.TransactionStatus);
                return;
            }

            if (IsStatus(transaction.TransactionStatus, StatusPending))
            {
                SetStatus(transaction, StatusCompleted);
                await dbContext.SaveChangesAsync(cancellationToken);
                return;
            }

            if (IsStatus(transaction.TransactionStatus, StatusRejected))
            {
                SetStatus(transaction, StatusPending);
                await dbContext.SaveChangesAsync(cancellationToken);

                SetStatus(transaction, StatusCompleted);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Transaction worker failed for {TransactionId} from {Source}.",
                item.TransactionId,
                item.Source);
        }
    }

    private static bool IsBlockedAccount(string accountStatus)
    {
        return string.Equals(accountStatus, AccountStatusBlocked, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFinalStatus(string status)
    {
        return IsStatus(status, StatusCompleted) || IsStatus(status, StatusFailed);
    }

    private static bool IsStatus(string status, string expected)
    {
        return string.Equals(status, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static void SetStatus(Transaction transaction, string status)
    {
        transaction.TransactionStatus = status;
        transaction.UpdatedAt = DateTime.UtcNow;
    }
}