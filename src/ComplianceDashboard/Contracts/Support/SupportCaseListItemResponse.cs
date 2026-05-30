namespace ComplianceDashboard.Contracts.Support;

public sealed record SupportCaseListItemResponse(
    string Id,
    string ClientName,
    string CaseType,
    string? Amount,
    string? RecipientName,
    string Status,
    string RouteTo,
    string? Summary);