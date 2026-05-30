namespace ComplianceDashboard.Entities;

public class Transaction
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid BankId { get; set; }

    public Guid IdempotencyKey { get; set; }

    public string Account { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string GatewayType { get; set; } = null!;

    public string TransactionStatus { get; set; } = null!;

    public int AiRiskScore { get; set; }

    public string AiRiskReason { get; set; } = null!;

    public string RawPayload { get; set; } = null!;

    public string? BankReferenceId { get; set; }

    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual BankAdapter Bank { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}