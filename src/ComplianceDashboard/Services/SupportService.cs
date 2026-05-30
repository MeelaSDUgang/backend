using System.Globalization;
using ComplianceDashboard.Contracts.Support;
using ComplianceDashboard.Data;
using ComplianceDashboard.Entities;
using ComplianceDashboard.Enums;
using Microsoft.EntityFrameworkCore;

namespace ComplianceDashboard.Services;

public class SupportService(DashboardDbContext dbContext) : ISupportService
{
    private static readonly AppealCaseStatus[] SupportVisibleStatuses =
    [
        AppealCaseStatus.SUBMITTED,
        AppealCaseStatus.WAITING_SUPPORT,
        AppealCaseStatus.NEED_MORE_INFO,
        AppealCaseStatus.RESOLVED,
        AppealCaseStatus.ESCALATED
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
        var appealCase = await LoadFullCase(caseId)
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

        var appealCase = await dbContext.AppealCases
            .Include(appealCase => appealCase.Operation)
            .FirstOrDefaultAsync(appealCase => appealCase.Id == caseId, cancellationToken);

        if (appealCase is null)
            return ServiceResult<SubmitSupportDecisionResponse>.Failure(ErrorCodes.NotFound, "Appeal case not found.");

        var now = DateTimeOffset.UtcNow;
        var supportDecision = new SupportDecision
        {
            Id = await GetNextIdAsync("decision", dbContext.SupportDecisions.Select(decision => decision.Id),
                cancellationToken),
            CaseId = appealCase.Id,
            Decision = decisionType,
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
                appealCase.Status.ToString(),
                appealCase.Operation?.Status.ToString()));
    }

    private IQueryable<AppealCase> LoadFullCase(string caseId)
    {
        return dbContext.AppealCases
            .Include(appealCase => appealCase.User)
            .Include(appealCase => appealCase.Operation)
            .Include(appealCase => appealCase.Answers)
            .Include(appealCase => appealCase.Documents)
            .Include(appealCase => appealCase.Decisions)
            .Where(appealCase => appealCase.Id == caseId);
    }

    private static SupportCaseListItemResponse ToListItem(AppealCase appealCase)
    {
        return new SupportCaseListItemResponse(
            appealCase.Id,
            appealCase.User.FullName,
            GetCaseTypeLabel(appealCase.CaseType),
            appealCase.Operation is null ? null : FormatAmount(appealCase.Operation.Amount),
            appealCase.Operation?.RecipientName,
            appealCase.Status.ToString(),
            appealCase.RouteTo.ToString(),
            appealCase.SupportSummary);
    }

    private static SupportCaseDetailsResponse ToDetails(AppealCase appealCase)
    {
        return new SupportCaseDetailsResponse(
            appealCase.Id,
            ResponseMapper.ToResponse(appealCase.User),
            appealCase.Operation is null ? null : ResponseMapper.ToResponse(appealCase.Operation),
            appealCase.CaseType.ToString(),
            appealCase.Status.ToString(),
            appealCase.RouteTo.ToString(),
            appealCase.Answers.OrderBy(answer => answer.CreatedAt).Select(ResponseMapper.ToResponse).ToArray(),
            appealCase.Documents.OrderBy(document => document.CreatedAt).Select(ResponseMapper.ToResponse).ToArray(),
            appealCase.SupportSummary,
            ResponseMapper.ParseMissingInfo(appealCase.MissingInfoJson),
            appealCase.ClientMessage,
            appealCase.Decisions.OrderBy(decision => decision.CreatedAt).Select(ResponseMapper.ToResponse).ToArray());
    }

    private static void ApplyDecision(AppealCase appealCase, SupportDecisionType decision)
    {
        switch (decision)
        {
            case SupportDecisionType.CONFIRM_OPERATION:
                appealCase.Status = AppealCaseStatus.RESOLVED;
                if (appealCase.Operation is not null)
                {
                    appealCase.Operation.Status = OperationStatus.SUCCESS;
                    appealCase.Operation.UpdatedAt = DateTimeOffset.UtcNow;
                }

                break;

            case SupportDecisionType.REQUEST_MORE_INFO:
                appealCase.Status = AppealCaseStatus.NEED_MORE_INFO;
                break;

            case SupportDecisionType.KEEP_BLOCKED:
                appealCase.Status = AppealCaseStatus.RESOLVED;
                if (appealCase.Operation is not null)
                {
                    appealCase.Operation.Status = OperationStatus.BLOCKED;
                    appealCase.Operation.UpdatedAt = DateTimeOffset.UtcNow;
                }

                break;

            case SupportDecisionType.ESCALATE:
                appealCase.Status = AppealCaseStatus.ESCALATED;
                break;
        }
    }

    private static string GetCaseTypeLabel(AppealCaseType caseType)
    {
        return caseType == AppealCaseType.OPERATION_CONFIRMATION
            ? "Подтверждение операции"
            : "Ограничение счета";
    }

    private static bool TryParseEnum<TEnum>(string value, out TEnum result)
        where TEnum : struct, Enum
    {
        return Enum.TryParse(value, false, out result) && Enum.IsDefined(result);
    }

    private static async Task<string> GetNextIdAsync(
        string prefix,
        IQueryable<string> ids,
        CancellationToken cancellationToken)
    {
        var values = await ids.ToListAsync(cancellationToken);
        var expectedPrefix = $"{prefix}_";
        var usedNumbers = values
            .Where(id => id.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            .Select(id => int.TryParse(id[expectedPrefix.Length..], out var number) ? number : 0)
            .Where(number => number > 0)
            .ToHashSet();

        var nextNumber = 1;
        while (usedNumbers.Contains(nextNumber)) nextNumber++;

        return $"{prefix}_{nextNumber}";
    }

    private static string FormatAmount(decimal amount)
    {
        return amount.ToString("#,0", CultureInfo.InvariantCulture).Replace(",", " ", StringComparison.Ordinal) + " ₸";
    }
}