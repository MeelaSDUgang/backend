namespace ComplianceDashboard.Entities;

public class User
{
    public string Id { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string AccountStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AppealCase> AppealCases { get; set; } = new List<AppealCase>();

    public virtual ICollection<Operation> Operations { get; set; } = new List<Operation>();
}