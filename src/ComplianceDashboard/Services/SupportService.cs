using System.Globalization;
using ComplianceDashboard.Contracts.Support;
using ComplianceDashboard.Data;
using ComplianceDashboard.Entities;
using ComplianceDashboard.Enums;
using Microsoft.EntityFrameworkCore;

namespace ComplianceDashboard.Services;

public class SupportService(DashboardDbContext dbContext) : ISupportService
{
    private static readonly string[] SupportVisibleStatuses =
    [
        nameof(AppealCaseStatus.SUBMITTED),
        nameof(AppealCaseStatus.WAITING_SUPPORT),
        nameof(AppealCaseStatus.NEED_MORE_INFO),
        nameof(AppealCaseStatus.RESOLVED)
    ];

    public async Task<IReadOnlyCollection<SupportCaseListItemResponse>> GetSupportCasesAsync(
        CancellationToken cancellationToken)
    {
        var cases = await dbContext.AppealCases
            .AsNoTracking()
            .Include(appealCase => appealCase.User)
            .Include(appealCase => appealCase.Operation)
            .Where(appealCase => SupportVisibleStatuses.Contains(appealCase.Status))
            .OrderByDescending(appealCase => appealCase.UpdatedAt)
            .ToListAsync(cancellationToken);

        return cases.Select(ToListItem).ToArray();
    }

    public async Task<ServiceResult<SupportCaseDetailsResponse>> GetSupportCaseAsync(
        string caseId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(caseId, out var parsedCaseId))
            return ServiceResult<SupportCaseDetailsResponse>.Failure(ErrorCodes.NotFound, "Appeal case not found.");

        var appealCase = await LoadFullCase(parsedCaseId)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return appealCase is null
            ? ServiceResult<SupportCaseDetailsResponse>.Failure(ErrorCodes.NotFound, "Appeal case not found.")
            : ServiceResult<SupportCaseDetailsResponse>.Success(ToDetails(appealCase));
    }

    public async Task<ServiceResult<SubmitSupportDecisionResponse>> SubmitDecisionAsync(
        string caseId,
        SubmitSupportDecisionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseEnum<SupportDecisionType>(request.Decision, out var decisionType))
            return ServiceResult<SubmitSupportDecisionResponse>.Failure(ErrorCodes.ValidationError,
                "Invalid decision.");

        if (!Guid.TryParse(caseId, out var parsedCaseId))
            return ServiceResult<SubmitSupportDecisionResponse>.Failure(ErrorCodes.NotFound, "Appeal case not found.");

        var appealCase = await dbContext.AppealCases
            .Include(appealCase => appealCase.Operation)
            .FirstOrDefaultAsync(appealCase => appealCase.Id == parsedCaseId, cancellationToken);

        if (appealCase is null)
            return ServiceResult<SubmitSupportDecisionResponse>.Failure(ErrorCodes.NotFound, "Appeal case not found.");

        var now = DateTime.UtcNow;
        var supportDecision = new SupportDecision
        {
            Id = Guid.NewGuid(),
            CaseId = appealCase.Id,
            Decision = decisionType.ToString(),
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            CreatedAt = now
        };

        ApplyDecision(appealCase, decisionType);
        appealCase.UpdatedAt = now;
        dbContext.SupportDecisions.Add(supportDecision);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<SubmitSupportDecisionResponse>.Success(
            new SubmitSupportDecisionResponse(
                true,
                appealCase.Status,
                appealCase.Operation?.Status));
    }

    private IQueryable<AppealCase> LoadFullCase(Guid caseId)
    {
        return dbContext.AppealCases
            .Include(appealCase => appealCase.User)
            .Include(appealCase => appealCase.Operation)
            .Include(appealCase => appealCase.AppealAnswers)
            .Include(appealCase => appealCase.AppealDocuments)
            .Include(appealCase => appealCase.SupportDecisions)
            .Where(appealCase => appealCase.Id == caseId);
    }

    private static SupportCaseListItemResponse ToListItem(AppealCase appealCase)
    {
        return new SupportCaseListItemResponse(
            appealCase.Id.ToString(),
            appealCase.User.FullName,
            GetCaseTypeLabel(appealCase.CaseType),
            appealCase.Operation is null ? null : FormatAmount(appealCase.Operation.Amount),
            appealCase.Operation?.RecipientName,
            appealCase.Status,
            appealCase.RouteTo,
            appealCase.SupportSummary);
    }

    private static SupportCaseDetailsResponse ToDetails(AppealCase appealCase)
    {
        return new SupportCaseDetailsResponse(
            appealCase.Id.ToString(),
            ResponseMapper.ToResponse(appealCase.User),
            appealCase.Operation is null ? null : ResponseMapper.ToResponse(appealCase.Operation),
            appealCase.CaseType,
            appealCase.Status,
            appealCase.RouteTo,
            appealCase.AppealAnswers.OrderBy(answer => answer.CreatedAt).Select(ResponseMapper.ToResponse).ToArray(),
            appealCase.AppealDocuments.OrderBy(document => document.CreatedAt).Select(ResponseMapper.ToResponse)
                .ToArray(),
            appealCase.SupportSummary,
            ResponseMapper.ParseMissingInfo(appealCase.MissingInfoJson),
            appealCase.ClientMessage,
            appealCase.SupportDecisions.OrderBy(decision => decision.CreatedAt).Select(ResponseMapper.ToResponse)
                .ToArray());
    }

    private static void ApplyDecision(AppealCase appealCase, SupportDecisionType decision)
    {
        switch (decision)
        {
            case SupportDecisionType.CONFIRM_OPERATION:
                appealCase.Status = AppealCaseStatus.RESOLVED.ToString();
                if (appealCase.Operation is not null)
                {
                    appealCase.Operation.Status = OperationStatus.SUCCESS.ToString();
                    appealCase.Operation.UpdatedAt = DateTime.UtcNow;
                }

                break;

            case SupportDecisionType.REQUEST_MORE_INFO:
                appealCase.Status = AppealCaseStatus.NEED_MORE_INFO.ToString();
                break;

            case SupportDecisionType.KEEP_BLOCKED:
                appealCase.Status = AppealCaseStatus.RESOLVED.ToString();
                if (appealCase.Operation is not null)
                {
                    appealCase.Operation.Status = OperationStatus.BLOCKED.ToString();
                    appealCase.Operation.UpdatedAt = DateTime.UtcNow;
                }

                break;

            case SupportDecisionType.ESCALATE:
                appealCase.Status = AppealCaseStatus.WAITING_SUPPORT.ToString();
                appealCase.RouteTo = RouteTo.ANTIFRAUD.ToString();
                break;
        }
    }

    private static string GetCaseTypeLabel(string caseType)
    {
        return caseType == AppealCaseType.OPERATION_CONFIRMATION.ToString()
            ? "Подтверждение операции"
            : "Ограничение счета";
    }

    private static bool TryParseEnum<TEnum>(string value, out TEnum result)
        where TEnum : struct, Enum
    {
        return Enum.TryParse(value, false, out result) && Enum.IsDefined(result);
    }

    private static string FormatAmount(decimal amount)
    {
        return amount.ToString("#,0", CultureInfo.InvariantCulture).Replace(",", " ", StringComparison.Ordinal) + " ₸";
    }
}
