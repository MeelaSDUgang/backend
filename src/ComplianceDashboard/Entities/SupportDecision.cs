using System;
using System.Collections.Generic;

namespace ComplianceDashboard.Entities;

public partial class SupportDecision
{
    public Guid Id { get; set; }

    public Guid CaseId { get; set; }

    public string Decision { get; set; } = null!;

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AppealCase Case { get; set; } = null!;
}
