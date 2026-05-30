using System.Text.Json;
using ComplianceDashboard.Contracts.Appeals;
using ComplianceDashboard.Entities;

namespace ComplianceDashboard.Services;

internal static class ResponseMapper
{
    public static UserResponse ToResponse(User user)
    {
        return new UserResponse(user.Id, user.FullName, user.Phone, user.AccountStatus);
    }

    public static OperationResponse ToResponse(Operation operation)
    {
        return new OperationResponse(
            operation.Id,
            operation.UserId,
            operation.Amount,
            operation.Currency,
            operation.RecipientName,
            operation.RecipientAccount,
            operation.Status,
            operation.BlockReasonCode,
            operation.CreatedAt);
    }

    public static AppealAnswerResponse ToResponse(AppealAnswer answer)
    {
        return new AppealAnswerResponse(
            answer.Id,
            answer.CaseId,
            answer.QuestionKey,
            answer.QuestionText,
            answer.Answer,
            answer.CreatedAt);
    }

    public static AppealDocumentResponse ToResponse(AppealDocument document)
    {
        return new AppealDocumentResponse(
            document.Id,
            document.CaseId,
            document.DocumentType,
            document.FileName,
            document.MockUrl,
            document.CreatedAt);
    }

    public static SupportDecisionResponse ToResponse(SupportDecision decision)
    {
        return new SupportDecisionResponse(
            decision.Id,
            decision.CaseId,
            decision.Decision,
            decision.Comment,
            decision.CreatedAt);
    }

    public static AppealCaseResponse ToResponse(AppealCase appealCase, bool includeNestedData = true)
    {
        return new AppealCaseResponse(
            appealCase.Id,
            appealCase.UserId,
            appealCase.OperationId,
            appealCase.CaseType,
            appealCase.Status,
            appealCase.RouteTo,
            appealCase.SupportSummary,
            appealCase.ClientMessage,
            ParseMissingInfo(appealCase.MissingInfoJson),
            includeNestedData && appealCase.User is not null ? ToResponse(appealCase.User) : null,
            includeNestedData && appealCase.Operation is not null ? ToResponse(appealCase.Operation) : null,
            appealCase.AppealAnswers.OrderBy(answer => answer.CreatedAt).Select(ToResponse).ToArray(),
            appealCase.AppealDocuments.OrderBy(document => document.CreatedAt).Select(ToResponse).ToArray(),
            appealCase.SupportDecisions.OrderBy(decision => decision.CreatedAt).Select(ToResponse).ToArray(),
            appealCase.CreatedAt,
            appealCase.UpdatedAt);
    }

    public static IReadOnlyCollection<string> ParseMissingInfo(string? missingInfoJson)
    {
        if (string.IsNullOrWhiteSpace(missingInfoJson)) return [];

        try
        {
            return JsonSerializer.Deserialize<string[]>(missingInfoJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}