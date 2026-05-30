namespace ComplianceDashboard.Contracts.Appeals;

/// <summary>
///     Request for saving all clarification answers for an appeal case.
/// </summary>
/// <param name="Answers">Non-empty collection of answers. Previous answers for the case are replaced.</param>
public sealed record SaveAppealAnswersRequest(IReadOnlyCollection<AppealAnswerRequest> Answers);

/// <summary>
///     One clarification answer from the client.
/// </summary>
/// <param name="QuestionKey">Stable key such as client_confirmed_operation, payment_purpose, recipient_relation.</param>
/// <param name="QuestionText">Question text shown to the client.</param>
/// <param name="Answer">Client answer.</param>
public sealed record AppealAnswerRequest(string QuestionKey, string QuestionText, string Answer);