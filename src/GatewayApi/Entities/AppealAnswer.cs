namespace GatewayApi.Entities;

public class AppealAnswer
{
    public Guid Id { get; set; }

    public Guid CaseId { get; set; }

    public AppealCase Case { get; set; } = null!;

    public string QuestionKey { get; set; } = string.Empty;

    public string QuestionText { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}