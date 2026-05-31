using System.Globalization;
using System.Text.Json;
using ComplianceDashboard.Contracts.Appeals;
using ComplianceDashboard.Data;
using ComplianceDashboard.Entities;
using ComplianceDashboard.Enums;
using Microsoft.EntityFrameworkCore;

namespace ComplianceDashboard.Services;

public class ClientAppealService(DashboardDbContext dbContext) : IClientAppealService
{
    public async Task<ServiceResult<UserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

        return user is null
            ? ServiceResult<UserResponse>.Failure(ErrorCodes.NotFound, "User not found.")
            : ServiceResult<UserResponse>.Success(ResponseMapper.ToResponse(user));
    }

    public async Task<ServiceResult<OperationResponse>> GetBlockedOperationAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var operation = await dbContext.Operations
            .AsNoTracking()
            .Where(operation => operation.UserId == userId)
            .Where(operation => operation.Status == OperationStatus.PENDING_CONFIRMATION.ToString() ||
                                operation.Status == OperationStatus.BLOCKED.ToString())
            .OrderByDescending(operation => operation.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return operation is null
            ? ServiceResult<OperationResponse>.Failure(ErrorCodes.NotFound, "Blocked operation not found.")
            : ServiceResult<OperationResponse>.Success(ResponseMapper.ToResponse(operation));
    }

    public async Task<ServiceResult<AppealCaseResponse>> CreateAppealCaseAsync(
        Guid userId,
        CreateAppealCaseRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseEnum<AppealCaseType>(request.CaseType, out var caseType))
            return ServiceResult<AppealCaseResponse>.Failure(ErrorCodes.ValidationError, "Invalid caseType.");

        Operation? operation = null;

        if (caseType == AppealCaseType.OPERATION_CONFIRMATION)
        {
            if (string.IsNullOrWhiteSpace(request.OperationId) ||
                !Guid.TryParse(request.OperationId, out var operationId))
                return ServiceResult<AppealCaseResponse>.Failure(
                    ErrorCodes.ValidationError,
                    "Valid operationId is required for OPERATION_CONFIRMATION.");

            operation = await dbContext.Operations
                .FirstOrDefaultAsync(
                    operation => operation.Id == operationId && operation.UserId == userId,
                    cancellationToken);

            if (operation is null)
                return ServiceResult<AppealCaseResponse>.Failure(ErrorCodes.NotFound, "Operation not found.");
        }

        var userExists = await dbContext.Users.AnyAsync(user => user.Id == userId, cancellationToken);
        if (!userExists) return ServiceResult<AppealCaseResponse>.Failure(ErrorCodes.NotFound, "User not found.");

        var now = DateTime.UtcNow;
        var appealCase = new AppealCase
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OperationId = operation?.Id,
            CaseType = caseType.ToString(),
            Status = AppealCaseStatus.DRAFT.ToString(),
            RouteTo = RouteTo.SUPPORT.ToString(),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.AppealCases.Add(appealCase);
        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.Entry(appealCase).Reference(item => item.User).LoadAsync(cancellationToken);
        if (appealCase.OperationId is not null)
            await dbContext.Entry(appealCase).Reference(item => item.Operation).LoadAsync(cancellationToken);

        return ServiceResult<AppealCaseResponse>.Success(ResponseMapper.ToResponse(appealCase));
    }

    public async Task<ServiceResult<AppealCaseResponse>> GetAppealCaseAsync(
        Guid userId,
        string caseId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(caseId, out var parsedCaseId))
            return ServiceResult<AppealCaseResponse>.Failure(ErrorCodes.NotFound, "Appeal case not found.");

        var appealCase = await LoadFullCase(parsedCaseId, userId)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return appealCase is null
            ? ServiceResult<AppealCaseResponse>.Failure(ErrorCodes.NotFound, "Appeal case not found.")
            : ServiceResult<AppealCaseResponse>.Success(ResponseMapper.ToResponse(appealCase));
    }

    public async Task<ServiceResult<object>> SaveAnswersAsync(
        Guid userId,
        string caseId,
        SaveAppealAnswersRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(caseId, out var parsedCaseId))
            return ServiceResult<object>.Failure(ErrorCodes.NotFound, "Appeal case not found.");

        var appealCase = await dbContext.AppealCases
            .Include(appealCase => appealCase.AppealAnswers)
            .FirstOrDefaultAsync(
                appealCase => appealCase.Id == parsedCaseId && appealCase.UserId == userId,
                cancellationToken);

        if (appealCase is null) return ServiceResult<object>.Failure(ErrorCodes.NotFound, "Appeal case not found.");

        if (appealCase.Status != AppealCaseStatus.DRAFT.ToString())
            return ServiceResult<object>.Failure(ErrorCodes.CaseAlreadySubmitted, "Case is already submitted.");

        if (request.Answers.Count == 0)
            return ServiceResult<object>.Failure(ErrorCodes.ValidationError, "answers must be a non-empty array.");

        foreach (var answer in request.Answers)
            if (string.IsNullOrWhiteSpace(answer.QuestionKey) ||
                string.IsNullOrWhiteSpace(answer.QuestionText) ||
                string.IsNullOrWhiteSpace(answer.Answer))
                return ServiceResult<object>.Failure(
                    ErrorCodes.ValidationError,
                    "Each answer requires questionKey, questionText and answer.");

        dbContext.AppealAnswers.RemoveRange(appealCase.AppealAnswers);
        var now = DateTime.UtcNow;

        foreach (var answer in request.Answers)
            dbContext.AppealAnswers.Add(new AppealAnswer
            {
                Id = Guid.NewGuid(),
                CaseId = appealCase.Id,
                QuestionKey = answer.QuestionKey.Trim(),
                QuestionText = answer.QuestionText.Trim(),
                Answer = answer.Answer.Trim(),
                CreatedAt = now
            });

        appealCase.UpdatedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<object>.Success(new { ok = true });
    }

    public async Task<ServiceResult<AppealDocumentResponse>> AddDocumentAsync(
        Guid userId,
        string caseId,
        AddAppealDocumentRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(caseId, out var parsedCaseId))
            return ServiceResult<AppealDocumentResponse>.Failure(ErrorCodes.NotFound, "Appeal case not found.");

        var appealCase = await dbContext.AppealCases
            .FirstOrDefaultAsync(
                appealCase => appealCase.Id == parsedCaseId && appealCase.UserId == userId,
                cancellationToken);

        if (appealCase is null)
            return ServiceResult<AppealDocumentResponse>.Failure(ErrorCodes.NotFound, "Appeal case not found.");

        if (appealCase.Status != AppealCaseStatus.DRAFT.ToString())
            return ServiceResult<AppealDocumentResponse>.Failure(ErrorCodes.CaseAlreadySubmitted,
                "Case is already submitted.");

        if (!TryParseEnum<DocumentType>(request.DocumentType, out var documentType))
            return ServiceResult<AppealDocumentResponse>.Failure(ErrorCodes.ValidationError, "Invalid documentType.");

        if (string.IsNullOrWhiteSpace(request.FileName))
            return ServiceResult<AppealDocumentResponse>.Failure(ErrorCodes.ValidationError, "fileName is required.");

        var now = DateTime.UtcNow;
        var document = new AppealDocument
        {
            Id = Guid.NewGuid(),
            CaseId = appealCase.Id,
            DocumentType = documentType.ToString(),
            FileName = request.FileName.Trim(),
            MockUrl = $"/mock-files/{request.FileName.Trim()}",
            CreatedAt = now
        };

        appealCase.UpdatedAt = now;
        dbContext.AppealDocuments.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<AppealDocumentResponse>.Success(ResponseMapper.ToResponse(document));
    }

    public async Task<ServiceResult<GenerateSupportSummaryResponse>> GenerateSupportSummaryAsync(
        Guid userId,
        string caseId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(caseId, out var parsedCaseId))
            return ServiceResult<GenerateSupportSummaryResponse>.Failure(ErrorCodes.NotFound, "Appeal case not found.");

        var appealCase = await LoadFullCase(parsedCaseId, userId).FirstOrDefaultAsync(cancellationToken);
        if (appealCase is null)
            return ServiceResult<GenerateSupportSummaryResponse>.Failure(ErrorCodes.NotFound, "Appeal case not found.");

        if (appealCase.Status != AppealCaseStatus.DRAFT.ToString())
            return ServiceResult<GenerateSupportSummaryResponse>.Failure(
                ErrorCodes.CaseAlreadySubmitted,
                "Case is already submitted.");

        var answers = appealCase.AppealAnswers.ToDictionary(answer => answer.QuestionKey, answer => answer.Answer);
        var confirmation = GetAnswer(answers, "client_confirmed_operation");
        var paymentPurpose = GetAnswer(answers, "payment_purpose_other");
        if (string.IsNullOrWhiteSpace(paymentPurpose)) paymentPurpose = GetAnswer(answers, "payment_purpose");

        var recipientRelation = GetAnswer(answers, "recipient_relation");
        var missingInfo = BuildMissingInfo(appealCase, paymentPurpose, recipientRelation);
        var routeTo = ResolveRoute(appealCase.CaseType, confirmation, recipientRelation);
        var supportSummary = BuildSupportSummary(appealCase, confirmation, paymentPurpose, recipientRelation);
        var clientMessage = BuildClientMessage(confirmation);

        appealCase.Status = AppealCaseStatus.SUBMITTED.ToString();
        appealCase.RouteTo = routeTo.ToString();
        appealCase.SupportSummary = supportSummary;
        appealCase.ClientMessage = clientMessage;
        appealCase.MissingInfoJson = JsonSerializer.Serialize(missingInfo);
        appealCase.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<GenerateSupportSummaryResponse>.Success(
            new GenerateSupportSummaryResponse(
                appealCase.Id.ToString(),
                appealCase.Status,
                appealCase.RouteTo,
                supportSummary,
                missingInfo,
                clientMessage));
    }

    private IQueryable<AppealCase> LoadFullCase(Guid caseId, Guid userId)
    {
        return dbContext.AppealCases
            .Include(appealCase => appealCase.User)
            .Include(appealCase => appealCase.Operation)
            .Include(appealCase => appealCase.AppealAnswers)
            .Include(appealCase => appealCase.AppealDocuments)
            .Include(appealCase => appealCase.SupportDecisions)
            .Where(appealCase => appealCase.Id == caseId && appealCase.UserId == userId);
    }

    private static string BuildSupportSummary(
        AppealCase appealCase,
        string? confirmation,
        string? paymentPurpose,
        string? recipientRelation)
    {
        if (appealCase.CaseType == AppealCaseType.ACCOUNT_BLOCK_APPEAL.ToString())
            return
                "Клиент подал обращение по ограничению счета. Специалисту нужно проверить пояснение клиента и запрошенные документы.";

        if (ContainsAny(confirmation, "нет", "no"))
            return
                "Клиент сообщил, что не совершал операцию. Информацию нужно передать на проверку в антифрод до принятия решения.";

        var amount = appealCase.Operation is null ? "заблокированную сумму" : FormatAmount(appealCase.Operation.Amount);
        var purpose = string.IsNullOrWhiteSpace(paymentPurpose) ? "не указано" : paymentPurpose;
        var relation = string.IsNullOrWhiteSpace(recipientRelation) ? "не указано" : recipientRelation;
        var documentsPart = appealCase.AppealDocuments.Count > 0
            ? $"Клиент приложил документ: {appealCase.AppealDocuments.First().FileName}."
            : "Подтверждающие документы не приложены.";

        return
            $"Клиент подтвердил операцию на {amount}. Указал назначение: {purpose}. Получатель — {relation}. {documentsPart} Требуется проверка приложенной информации и принятие решения по операции.";
    }

    private static string BuildClientMessage(string? confirmation)
    {
        if (ContainsAny(confirmation, "нет", "no"))
            return "Мы передали информацию в службу безопасности. Ограничение будет сохранено до проверки.";

        return
            "Спасибо. Мы передали ваши ответы и документы специалисту. Если потребуется дополнительная информация, мы сообщим в приложении.";
    }

    private static List<string> BuildMissingInfo(
        AppealCase appealCase,
        string? paymentPurpose,
        string? recipientRelation)
    {
        var missingInfo = new List<string>();

        if (appealCase.AppealDocuments.Count == 0) missingInfo.Add("Не приложены подтверждающие документы");

        var hasCheckOrContract = appealCase.AppealDocuments.Any(document =>
            document.DocumentType is "CHECK" or "CONTRACT");

        if (ContainsAny(paymentPurpose, "покупка товара", "purchase") && !hasCheckOrContract)
            missingInfo.Add("Чек оплаты или договор купли-продажи отсутствует");

        if (ContainsAny(recipientRelation, "не знаю", "do not know", "unknown"))
            missingInfo.Add("Клиент не знает получателя лично");

        return missingInfo;
    }

    private static RouteTo ResolveRoute(string caseType, string? confirmation, string? recipientRelation)
    {
        if (caseType == AppealCaseType.ACCOUNT_BLOCK_APPEAL.ToString()) return RouteTo.COMPLIANCE;

        if (ContainsAny(confirmation, "нет", "no") ||
            ContainsAny(recipientRelation, "продавец", "seller", "не знаю", "unknown"))
            return RouteTo.ANTIFRAUD;

        return RouteTo.SUPPORT;
    }

    private static string? GetAnswer(IReadOnlyDictionary<string, string> answers, string key)
    {
        return answers.TryGetValue(key, out var answer) ? answer : null;
    }

    private static bool ContainsAny(string? value, params string[] fragments)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               fragments.Any(fragment => value.Contains(fragment, StringComparison.OrdinalIgnoreCase));
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