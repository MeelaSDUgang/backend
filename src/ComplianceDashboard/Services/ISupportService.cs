using ComplianceDashboard.Contracts.Support;

namespace ComplianceDashboard.Services;

public interface ISupportService
{
    Task<IReadOnlyCollection<SupportCaseListItemResponse>> GetSupportCasesAsync(CancellationToken cancellationToken);

    Task<ServiceResult<SupportCaseDetailsResponse>> GetSupportCaseAsync(string caseId,
        CancellationToken cancellationToken);

    Task<ServiceResult<SubmitSupportDecisionResponse>> SubmitDecisionAsync(
        string caseId,
        SubmitSupportDecisionRequest request,
        CancellationToken cancellationToken);
}