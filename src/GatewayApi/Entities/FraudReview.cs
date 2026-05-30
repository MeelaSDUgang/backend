namespace GatewayApi.Entities;

public class FraudReview
{
    public Guid Id { get; set; }

    public Guid TransactionId { get; set; }

    public Transaction Transaction { get; set; } = null!;

    public decimal FraudScore { get; set; }

    public string RiskTier { get; set; } = string.Empty;

    public bool IsFraud { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string ReasonsJson { get; set; } = "[]";

    public string Advice { get; set; } = string.Empty;

    public DateTimeOffset EvaluatedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}