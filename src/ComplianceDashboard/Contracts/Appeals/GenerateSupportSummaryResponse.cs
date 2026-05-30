namespace ComplianceDashboard.Contracts.Appeals;

public sealed record GenerateSupportSummaryResponse(
    string CaseId,
    string Status,
    string RouteTo,
    string SupportSummary,
    IReadOnlyCollection<string> MissingInfo,
    string ClientMessage);