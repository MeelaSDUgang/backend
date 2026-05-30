using System;
using System.Collections.Generic;

namespace ComplianceDashboard.Entities;

public partial class Operation
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

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
