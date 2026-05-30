using ComplianceDashboard.Contracts.Appeals;

namespace ComplianceDashboard.Services;

public interface IClientAppealService
{
    Task<ServiceResult<UserResponse>> GetCurrentUserAsync(CancellationToken cancellationToken);

    Task<ServiceResult<OperationResponse>> GetBlockedOperationAsync(CancellationToken cancellationToken);

    Task<ServiceResult<AppealCaseResponse>> CreateAppealCaseAsync(
        CreateAppealCaseRequest request,
        CancellationToken cancellationToken);

    Task<ServiceResult<AppealCaseResponse>> GetAppealCaseAsync(string caseId, CancellationToken cancellationToken);

    Task<ServiceResult<object>> SaveAnswersAsync(
        string caseId,
        SaveAppealAnswersRequest request,
        CancellationToken cancellationToken);

    Task<ServiceResult<AppealDocumentResponse>> AddDocumentAsync(
        string caseId,
        AddAppealDocumentRequest request,
        CancellationToken cancellationToken);

    Task<ServiceResult<GenerateSupportSummaryResponse>> GenerateSupportSummaryAsync(
        string caseId,
        CancellationToken cancellationToken);
}