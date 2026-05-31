using GatewayApi.Enums;

namespace GatewayApi.Entities;

public class User
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string SecretKeyHash { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public AccountStatus AccountStatus { get; set; } = AccountStatus.ACTIVE;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Operation> Operations { get; set; } = [];

    public ICollection<AppealCase> AppealCases { get; set; } = [];

    public ICollection<Transaction> Transactions { get; set; } = [];
}