namespace ComplianceDashboard.Contracts.Appeals;

/// <summary>
///     Request for adding a mock appeal document.
/// </summary>
/// <param name="DocumentType">Allowed values: CHECK, CONTRACT, CHAT_SCREENSHOT, INVOICE, OTHER.</param>
/// <param name="FileName">File name to expose through mockUrl as /mock-files/{fileName}.</param>
public sealed record AddAppealDocumentRequest(string DocumentType, string FileName);