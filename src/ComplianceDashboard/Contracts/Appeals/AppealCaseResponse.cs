namespace ComplianceDashboard.Contracts.Appeals;

public sealed record AppealCaseResponse(
    string Id,
    string UserId,
    string? OperationId,
    string CaseType,
    string Status,
    string RouteTo,
    string? SupportSummary,
    string? ClientMessage,
    IReadOnlyCollection<string> MissingInfo,
    UserResponse? User,
    OperationResponse? Operation,
    IReadOnlyCollection<AppealAnswerResponse> Answers,
    IReadOnlyCollection<AppealDocumentResponse> Documents,
    IReadOnlyCollection<SupportDecisionResponse> Decisions,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UserResponse(
    string Id,
    string FullName,
    string Phone,
    string AccountStatus);

public sealed record OperationResponse(
    string Id,
    string UserId,
    decimal Amount,
    string Currency,
    string RecipientName,
    string? RecipientAccount,
    string Status,
    string BlockReasonCode,
    DateTimeOffset CreatedAt);

public sealed record AppealAnswerResponse(
    string Id,
    string CaseId,
    string QuestionKey,
    string QuestionText,
    string Answer,
    DateTimeOffset CreatedAt);

public sealed record AppealDocumentResponse(
    string Id,
    string CaseId,
    string DocumentType,
    string FileName,
    string? MockUrl,
    DateTimeOffset CreatedAt);

public sealed record SupportDecisionResponse(
    string Id,
    string CaseId,
    string Decision,
    string? Comment,
    DateTimeOffset CreatedAt);