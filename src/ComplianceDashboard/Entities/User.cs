using ComplianceDashboard.Enums;

namespace ComplianceDashboard.Entities;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public AccountStatus AccountStatus { get; set; } = AccountStatus.ACTIVE;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Operation> Operations { get; set; } = [];

    public ICollection<AppealCase> AppealCases { get; set; } = [];
}