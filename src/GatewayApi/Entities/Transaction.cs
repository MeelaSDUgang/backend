using GatewayApi.Enums;

namespace GatewayApi.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public Guid BankId { get; set; }
    public BankAdapter BankAdapter { get; set; }
    public Guid IdempotencyKey { get; set; }
    public string Account { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public GatewayType GatewayType { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public int AiRiskScore { get; set; }
    public string AiRiskReason { get; set; } = string.Empty;
    public string RawPayload { get; set; } = "{}";
    public string? BankReferenceId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}