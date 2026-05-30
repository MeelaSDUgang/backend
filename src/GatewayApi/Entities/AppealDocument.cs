using GatewayApi.Enums;

namespace GatewayApi.Entities;

public class AppealDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string CaseId { get; set; } = string.Empty;

    public AppealCase Case { get; set; } = null!;

    public DocumentType DocumentType { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string? MockUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}