using GatewayApi.Entities;

namespace GatewayApi.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!context.BankAdapters.Any())
        {
            var adapters = new List<BankAdapter>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    RoutingKey = "alpha.retail.hybrid",
                    Name = "Alpha Bank (P2P & A2A)",
                    IsActive = true,
                    SupportedGatewayTypes = "P2P,A2A"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    RoutingKey = "beta.corporate.b2b",
                    Name = "Beta Corporate Gateway (B2B)",
                    IsActive = true,
                    SupportedGatewayTypes = "B2B"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    RoutingKey = "retail.payout.b2c",
                    Name = "Retail Payout System (B2C)",
                    IsActive = true,
                    SupportedGatewayTypes = "B2C"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    RoutingKey = "global.fintech.universal",
                    Name = "Global FinTech Mock (All Types)",
                    IsActive = true,
                    SupportedGatewayTypes = "P2P,A2A,B2B,B2C"
                }
            };

            await context.BankAdapters.AddRangeAsync(adapters);
        }

        if (!context.Merchants.Any())
        {
            var testMerchant = new Merchant
            {
                Id = Guid.NewGuid(),
                Name = "Test Shop Inc",
                ApiKey = "nx_merch_test_12345",
                SecretKeyHash = "nx_merch_test_12345_secret_key",
                CreatedAt = DateTime.UtcNow
            };

            await context.Merchants.AddAsync(testMerchant);
        }

        await context.SaveChangesAsync();
    }
}