namespace ComplianceDashboard.Services.TransactionProcessing;

public interface ILowRiskPollingService
{
    Task TriggerAsync(Guid transactionId, CancellationToken cancellationToken);
}