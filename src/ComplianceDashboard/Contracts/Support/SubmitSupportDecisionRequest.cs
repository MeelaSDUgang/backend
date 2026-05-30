namespace ComplianceDashboard.Contracts.Support;

/// <summary>
///     Request for submitting a support decision.
/// </summary>
/// <param name="Decision">Allowed values: CONFIRM_OPERATION, REQUEST_MORE_INFO, KEEP_BLOCKED, ESCALATE.</param>
/// <param name="Comment">Optional support operator comment.</param>
public sealed record SubmitSupportDecisionRequest(string Decision, string? Comment);