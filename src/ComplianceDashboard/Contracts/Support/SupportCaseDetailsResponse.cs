using ComplianceDashboard.Contracts.Appeals;

namespace ComplianceDashboard.Contracts.Support;

public sealed record SupportCaseDetailsResponse(
    string Id,
    UserResponse Client,
    OperationResponse? Operation,
    string CaseType,
    string Status,
    string RouteTo,
    IReadOnlyCollection<AppealAnswerResponse> Answers,
    IReadOnlyCollection<AppealDocumentResponse> Documents,
    string? SupportSummary,
    IReadOnlyCollection<string> MissingInfo,
    string? ClientMessage,
    IReadOnlyCollection<SupportDecisionResponse> Decisions);