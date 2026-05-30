namespace ComplianceDashboard.Contracts.Support;

public sealed record SubmitSupportDecisionResponse(
    bool Ok,
    string CaseStatus,
    string? OperationStatus);