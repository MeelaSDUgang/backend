namespace GatewayApi.Services.Banking.Adapters;

public class RetailPayoutAdapter : IPaymentAdapter
{
    public string RoutingKey => "retail.payout.b2c";

    public Task<BankResponse> ProcessPaymentAsync(PaymentContext context, CancellationToken ct = default)
    {
        var refId = $"RETAIL-PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        return Task.FromResult(new BankResponse(true, refId));
    }
}