namespace ComplianceDashboard.Services.TransactionProcessing;

public sealed record TransactionWorkItem(Guid TransactionId, string Source);