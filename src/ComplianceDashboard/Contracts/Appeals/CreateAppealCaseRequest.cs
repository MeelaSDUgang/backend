namespace ComplianceDashboard.Contracts.Appeals;

/// <summary>
///     Request for creating a client appeal case.
/// </summary>
/// <param name="OperationId">
///     Operation id. Required when <paramref name="CaseType" /> is OPERATION_CONFIRMATION.
///     Example: op_1.
/// </param>
/// <param name="CaseType">
///     Appeal case type. Allowed values: OPERATION_CONFIRMATION, ACCOUNT_BLOCK_APPEAL.
/// </param>
public sealed record CreateAppealCaseRequest(string? OperationId, string CaseType);