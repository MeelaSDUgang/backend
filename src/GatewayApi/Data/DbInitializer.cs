using System.Security.Cryptography;
using GatewayApi.Entities;
using GatewayApi.Enums;

namespace GatewayApi.Data;

public static class DbInitializer
{
    private const string DefaultPhone = "77770000000";
    private const string DefaultPassword = "Password123";
    private static readonly Guid DefaultUserId = Guid.NewGuid();

    public static async Task SeedAsync(AppDbContext context)
    {
        var now = DateTimeOffset.UtcNow;

        var defaultUser = context.Users.FirstOrDefault(user => user.Phone == DefaultPhone);
        if (defaultUser is null)
            context.Users.Add(new User
            {
                Id = DefaultUserId,
                FullName = "maksat",
                Phone = DefaultPhone,
                ApiKey = "demo-user-api-key",
                SecretKeyHash = "demo-user-secret-hash",
                PasswordHash = HashPassword(DefaultPassword),
                AccountStatus = AccountStatus.ACTIVE,
                CreatedAt = now,
                UpdatedAt = now
            });

        if (!context.BankAdapters.Any())
        {
            var adapters = new List<BankAdapter>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    RoutingKey = "halykP2P",
                    Name = "Halyk Bank",
                    IsActive = true,
                    SupportedGatewayTypes = "P2P"
                }
            };

            await context.BankAdapters.AddRangeAsync(adapters);
        }

        await context.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        const int iterations = 100_000;
        const int saltSize = 16;
        const int keySize = 32;

        var salt = RandomNumberGenerator.GetBytes(saltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            keySize);

        return $"PBKDF2-SHA256${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }
}