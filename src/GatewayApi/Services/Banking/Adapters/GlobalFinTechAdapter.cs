using GatewayApi.Entities;

namespace GatewayApi.Services.Banking.Adapters;

public class GlobalFinTechAdapter : IPaymentAdapter
{
    public string RoutingKey => "global.fintech.universal";

    public Task<BankResponse> ProcessPaymentAsync(PaymentContext context, CancellationToken ct = default)
    {
        var prefix = context.Type switch
        {
            GatewayType.P2P => "GFT-P2P",
            GatewayType.A2A => "GFT-A2A",
            GatewayType.B2B => "GFT-B2B",
            GatewayType.B2C => "GFT-B2C",
            _ => "GFT-UNK"
        };

        var refId = $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        return Task.FromResult(new BankResponse(true, refId));
    }
}