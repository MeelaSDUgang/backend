namespace GatewayApi.Entities;

public class Merchant
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string ApiKey { get; set; }
    public string SecretKeyHash { get; set; }
    public string? WebhookUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}