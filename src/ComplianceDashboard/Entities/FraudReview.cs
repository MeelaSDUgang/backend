namespace ComplianceDashboard.Entities;

public class FraudReview
{
    public Guid Id { get; set; }

    public Guid TransactionId { get; set; }

    public decimal FraudScore { get; set; }

    public string RiskTier { get; set; } = null!;

    public bool IsFraud { get; set; }

    public string Status { get; set; } = null!;

    public string Summary { get; set; } = null!;

    public string ReasonsJson { get; set; } = null!;

    public string Advice { get; set; } = null!;

    public DateTime EvaluatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}