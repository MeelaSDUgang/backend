using System;
using System.Collections.Generic;

namespace ComplianceDashboard.Entities;

public partial class BankAdapter
{
    public Guid Id { get; set; }

    public string RoutingKey { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public string SupportedGatewayTypes { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
