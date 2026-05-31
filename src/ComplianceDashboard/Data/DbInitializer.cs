using ComplianceDashboard.Entities;
using ComplianceDashboard.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace ComplianceDashboard.Data;

public static class DbInitializer
{
    private static readonly Guid DefaultUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid LowRiskOperationId = Guid.Parse("22222222-2222-2222-2222-222222222221");
    private static readonly Guid MediumRiskOperationId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private const string DefaultFullName = "Demo Client";
    private const string DefaultPhone = "+77010000000";
    private const string DefaultPassword = "123456";
    private const string DefaultApiKey = "compliance-dashboard-demo-api-key";
    private const string DefaultSecretKeyHash = "compliance-dashboard-demo-secret-hash";

    public static async Task SeedAsync(
        DashboardDbContext dbContext,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(
            user => user.Id == DefaultUserId || user.Phone == DefaultPhone || user.ApiKey == DefaultApiKey,
            cancellationToken);

        if (user is null)
        {
            var now = DateTime.UtcNow;
            user = new User
            {
                Id = DefaultUserId,
                FullName = DefaultFullName,
                Phone = DefaultPhone,
                ApiKey = DefaultApiKey,
                SecretKeyHash = DefaultSecretKeyHash,
                PasswordHash = passwordHasher.Hash(DefaultPassword),
                AccountStatus = "ACTIVE",
                CreatedAt = now,
                UpdatedAt = now
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var changed = false;

            if (user.FullName != DefaultFullName)
            {
                user.FullName = DefaultFullName;
                changed = true;
            }

            if (user.Phone != DefaultPhone)
            {
                user.Phone = DefaultPhone;
                changed = true;
            }

            if (user.ApiKey != DefaultApiKey)
            {
                user.ApiKey = DefaultApiKey;
                changed = true;
            }

            if (user.SecretKeyHash != DefaultSecretKeyHash)
            {
                user.SecretKeyHash = DefaultSecretKeyHash;
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(user.PasswordHash) ||
                !passwordHasher.Verify(DefaultPassword, user.PasswordHash))
            {
                user.PasswordHash = passwordHasher.Hash(DefaultPassword);
                changed = true;
            }

            if (user.AccountStatus != "ACTIVE")
            {
                user.AccountStatus = "ACTIVE";
                changed = true;
            }

            if (changed)
            {
                user.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        await EnsureBlockedOperationsAsync(dbContext, user.Id, cancellationToken);
    }

    public static async Task EnsureBlockedOperationsAsync(
        DashboardDbContext dbContext,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var existingBlockReasons = await dbContext.Operations
            .Where(operation => operation.UserId == userId)
            .Select(operation => operation.BlockReasonCode)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        if (!existingBlockReasons.Contains("CLIENT_CONFIRMATION_REQUIRED"))
            dbContext.Operations.Add(new Operation
            {
                Id = userId == DefaultUserId ? LowRiskOperationId : Guid.NewGuid(),
                UserId = userId,
                Amount = 15000m,
                Currency = "KZT",
                RecipientName = "Aidos K.",
                RecipientAccount = "KZ111111111111111111",
                Status = "PENDING_CONFIRMATION",
                BlockReasonCode = "CLIENT_CONFIRMATION_REQUIRED",
                CreatedAt = now,
                UpdatedAt = now
            });

        if (!existingBlockReasons.Contains("SUSPICIOUS_TRANSFER"))
            dbContext.Operations.Add(new Operation
            {
                Id = userId == DefaultUserId ? MediumRiskOperationId : Guid.NewGuid(),
                UserId = userId,
                Amount = 125000m,
                Currency = "KZT",
                RecipientName = "Nurlan A. (OLX)",
                RecipientAccount = "KZ222222222222222222",
                Status = "PENDING_CONFIRMATION",
                BlockReasonCode = "SUSPICIOUS_TRANSFER",
                CreatedAt = now.AddSeconds(1),
                UpdatedAt = now.AddSeconds(1)
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
