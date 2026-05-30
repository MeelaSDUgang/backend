using GatewayApi.Enums;

namespace GatewayApi.Entities;

public class SupportDecision
{
    public Guid Id { get; set; }

    public Guid CaseId { get; set; }

    public AppealCase Case { get; set; } = null!;

    public SupportDecisionType Decision { get; set; }

    public string? Comment { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}