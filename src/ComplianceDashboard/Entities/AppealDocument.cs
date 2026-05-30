namespace ComplianceDashboard.Entities;

public class AppealDocument
{
    public string Id { get; set; } = null!;

    public string CaseId { get; set; } = null!;

    public string DocumentType { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string? MockUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AppealCase Case { get; set; } = null!;
}