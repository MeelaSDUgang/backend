using ComplianceDashboard.Contracts.Appeals;

namespace ComplianceDashboard.Services;

public interface IClientAppealService
{
    Task<ServiceResult<UserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<ServiceResult<OperationResponse>> GetBlockedOperationAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OperationResponse>> GetBlockedOperationsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<ServiceResult<AppealCaseResponse>> CreateAppealCaseAsync(
        Guid userId,
        CreateAppealCaseRequest request,
        CancellationToken cancellationToken);

    Task<ServiceResult<AppealCaseResponse>> GetAppealCaseAsync(
        Guid userId,
        string caseId,
        CancellationToken cancellationToken);

    Task<ServiceResult<object>> SaveAnswersAsync(
        Guid userId,
        string caseId,
        SaveAppealAnswersRequest request,
        CancellationToken cancellationToken);

    Task<ServiceResult<AppealDocumentResponse>> AddDocumentAsync(
        Guid userId,
        string caseId,
        AddAppealDocumentRequest request,
        CancellationToken cancellationToken);

    Task<ServiceResult<GenerateSupportSummaryResponse>> GenerateSupportSummaryAsync(
        Guid userId,
        string caseId,
        CancellationToken cancellationToken);
}
