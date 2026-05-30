namespace GatewayApi.Entities;

public class AppealAnswer
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string CaseId { get; set; } = string.Empty;

    public AppealCase Case { get; set; } = null!;

    public string QuestionKey { get; set; } = string.Empty;

    public string QuestionText { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}