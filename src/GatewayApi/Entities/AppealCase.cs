using GatewayApi.Enums;

namespace GatewayApi.Entities;

public class AppealCase
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid? OperationId { get; set; }

    public Operation? Operation { get; set; }

    public AppealCaseType CaseType { get; set; }

    public AppealCaseStatus Status { get; set; } = AppealCaseStatus.DRAFT;

    public string? SupportSummary { get; set; }

    public string? ClientMessage { get; set; }

    public string? MissingInfoJson { get; set; }

    public RouteTo RouteTo { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<AppealAnswer> Answers { get; set; } = [];

    public ICollection<AppealDocument> Documents { get; set; } = [];

    public ICollection<SupportDecision> Decisions { get; set; } = [];
}