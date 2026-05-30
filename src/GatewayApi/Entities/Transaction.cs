using GatewayApi.Enums;

namespace GatewayApi.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid BankId { get; set; }
    public BankAdapter BankAdapter { get; set; } = null!;
    public Guid IdempotencyKey { get; set; }
    public string Account { get; set; } = null!; // по идее в идеале держать в хэше или в маске ну лан похуй
    public decimal Amount { get; set; }
    public string NameDest { get; set; } = string.Empty;
    public string NameOrig { get; set; } = string.Empty;
    public decimal NewbalanceDest { get; set; }
    public decimal NewbalanceOrig { get; set; }
    public decimal OldbalanceDest { get; set; }
    public decimal OldbalanceOrg { get; set; }
    public int Step { get; set; }
    public string Type { get; set; } = "PAYMENT";
    public string Label { get; set; } = "Legitimate PAYMENT";
    public string Currency { get; set; } = null!;
    public GatewayType GatewayType { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public FraudReview? FraudReview { get; set; }
}