using GatewayApi.Enums;

namespace GatewayApi.Entities;

public class AppealDocument
{
    public Guid Id { get; set; }

    public Guid CaseId { get; set; }

    public AppealCase Case { get; set; } = null!;

    public DocumentType DocumentType { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string? MockUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}