namespace ComplianceDashboard.Services.TransactionProcessing;

public class HighRiskVerificationService(ITransactionWorkerQueue queue) : IHighRiskVerificationService
{
    public Task TriggerAsync(Guid transactionId, CancellationToken cancellationToken)
    {
        return queue.EnqueueAsync(
            new TransactionWorkItem(transactionId, "high-risk-verification"),
            cancellationToken).AsTask();
    }
}