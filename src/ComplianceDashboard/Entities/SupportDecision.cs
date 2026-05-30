namespace ComplianceDashboard.Entities;

public class SupportDecision
{
    public string Id { get; set; } = null!;

    public string CaseId { get; set; } = null!;

    public string Decision { get; set; } = null!;

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AppealCase Case { get; set; } = null!;
}