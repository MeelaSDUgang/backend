namespace ComplianceDashboard.Services.TransactionProcessing;

public interface IHighRiskVerificationService
{
    Task TriggerAsync(Guid transactionId, CancellationToken cancellationToken);
}