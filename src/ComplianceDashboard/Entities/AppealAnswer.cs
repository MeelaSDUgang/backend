namespace ComplianceDashboard.Entities;

public class AppealAnswer
{
    public string Id { get; set; } = null!;

    public string CaseId { get; set; } = null!;

    public string QuestionKey { get; set; } = null!;

    public string QuestionText { get; set; } = null!;

    public string Answer { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual AppealCase Case { get; set; } = null!;
}