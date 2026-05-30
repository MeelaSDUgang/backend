namespace ComplianceDashboard.Entities;

public class Operation
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string RecipientName { get; set; } = null!;

    public string? RecipientAccount { get; set; }

    public string Status { get; set; } = null!;

    public string BlockReasonCode { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AppealCase> AppealCases { get; set; } = new List<AppealCase>();

    public virtual User User { get; set; } = null!;
}