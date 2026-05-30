namespace ComplianceDashboard.Entities;

public class User
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string ApiKey { get; set; } = null!;

    public string SecretKeyHash { get; set; } = null!;

    public string AccountStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AppealCase> AppealCases { get; set; } = new List<AppealCase>();

    public virtual ICollection<Operation> Operations { get; set; } = new List<Operation>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}