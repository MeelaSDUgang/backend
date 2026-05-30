using System;
using System.Collections.Generic;

namespace ComplianceDashboard.Entities;

public partial class Transaction
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid BankId { get; set; }

    public Guid IdempotencyKey { get; set; }

    public string Account { get; set; } = null!;

    public decimal Amount { get; set; }

    public string NameDest { get; set; } = null!;

    public string NameOrig { get; set; } = null!;

    public decimal NewbalanceDest { get; set; }

    public decimal NewbalanceOrig { get; set; }

    public decimal OldbalanceDest { get; set; }

    public decimal OldbalanceOrg { get; set; }

    public int Step { get; set; }

    public string Type { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public string GatewayType { get; set; } = null!;

    public string TransactionStatus { get; set; } = null!;

    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual BankAdapter Bank { get; set; } = null!;

    public virtual FraudReview? FraudReview { get; set; }

    public virtual User User { get; set; } = null!;
}
