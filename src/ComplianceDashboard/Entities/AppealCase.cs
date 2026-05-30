using System;
using System.Collections.Generic;

namespace ComplianceDashboard.Entities;

public partial class AppealCase
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? OperationId { get; set; }

    public string CaseType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? SupportSummary { get; set; }

    public string? ClientMessage { get; set; }

    public string? MissingInfoJson { get; set; }

    public string RouteTo { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AppealAnswer> AppealAnswers { get; set; } = new List<AppealAnswer>();

    public virtual ICollection<AppealDocument> AppealDocuments { get; set; } = new List<AppealDocument>();

    public virtual Operation? Operation { get; set; }

    public virtual ICollection<SupportDecision> SupportDecisions { get; set; } = new List<SupportDecision>();

    public virtual User User { get; set; } = null!;
}
