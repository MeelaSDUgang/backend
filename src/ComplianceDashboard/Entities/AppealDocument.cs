using System;
using System.Collections.Generic;

namespace ComplianceDashboard.Entities;

public partial class AppealDocument
{
    public Guid Id { get; set; }

    public Guid CaseId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string? MockUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AppealCase Case { get; set; } = null!;
}
