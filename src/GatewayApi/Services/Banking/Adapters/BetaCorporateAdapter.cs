namespace GatewayApi.Services.Banking.Adapters;

public class BetaCorporateAdapter : IPaymentAdapter
{
    public string RoutingKey => "beta.corporate.b2b";

    public Task<BankResponse> ProcessPaymentAsync(PaymentContext context, CancellationToken ct = default)
    {
        var refId = $"BETA-CORP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        return Task.FromResult(new BankResponse(true, refId));
    }
}