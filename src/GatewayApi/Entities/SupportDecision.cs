using GatewayApi.Enums;

namespace GatewayApi.Entities;

public class SupportDecision
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string CaseId { get; set; } = string.Empty;

    public AppealCase Case { get; set; } = null!;

    public SupportDecisionType Decision { get; set; }

    public string? Comment { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}