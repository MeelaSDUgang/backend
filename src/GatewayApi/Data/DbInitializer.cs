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
}