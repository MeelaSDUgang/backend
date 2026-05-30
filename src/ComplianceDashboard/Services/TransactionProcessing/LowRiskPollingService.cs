namespace ComplianceDashboard.Services.TransactionProcessing;

public class LowRiskPollingService(ITransactionWorkerQueue queue) : ILowRiskPollingService
{
    public Task TriggerAsync(Guid transactionId, CancellationToken cancellationToken)
    {
        return queue.EnqueueAsync(
            new TransactionWorkItem(transactionId, "low-risk-polling"),
            cancellationToken).AsTask();
    }
}