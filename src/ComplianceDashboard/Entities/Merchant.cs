namespace ComplianceDashboard.Entities;

public class Merchant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string ApiKey { get; set; } = null!;

    public string SecretKeyHash { get; set; } = null!;

    public string? WebhookUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}