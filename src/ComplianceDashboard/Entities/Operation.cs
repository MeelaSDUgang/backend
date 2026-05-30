using ComplianceDashboard.Enums;

namespace ComplianceDashboard.Entities;

public class Operation
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public decimal Amount { get; set; }

    public Currency Currency { get; set; } = Currency.KZT;

    public string RecipientName { get; set; } = string.Empty;

    public OperationStatus Status { get; set; }

    public BlockReasonCode BlockReasonCode { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<AppealCase> AppealCases { get; set; } = [];
}