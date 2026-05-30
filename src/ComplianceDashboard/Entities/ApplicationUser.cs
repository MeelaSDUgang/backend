using ComplianceDashboard.Enums;
using Microsoft.AspNetCore.Identity;

namespace ComplianceDashboard.Entities;

public class ApplicationUser : IdentityUser<string>
{
    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public AccountStatus AccountStatus { get; set; } = AccountStatus.ACTIVE;

    public ICollection<Operation> Operations { get; set; } = [];

    public ICollection<AppealCase> AppealCases { get; set; } = [];
}