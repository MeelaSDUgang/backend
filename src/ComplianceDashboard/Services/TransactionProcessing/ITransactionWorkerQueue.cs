namespace ComplianceDashboard.Services.TransactionProcessing;

public interface ITransactionWorkerQueue
{
    ValueTask EnqueueAsync(TransactionWorkItem item, CancellationToken cancellationToken);

    ValueTask<TransactionWorkItem> DequeueAsync(CancellationToken cancellationToken);
}