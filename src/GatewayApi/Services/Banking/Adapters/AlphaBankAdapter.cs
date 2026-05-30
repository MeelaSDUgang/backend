namespace GatewayApi.Services.Banking.Adapters;

public class AlphaBankAdapter : IPaymentAdapter
{
    public string RoutingKey => "alpha.retail.hybrid";

    public Task<BankResponse> ProcessPaymentAsync(PaymentContext context, CancellationToken ct = default)
    {
        var refId = $"ALPHA-{context.Type}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        return Task.FromResult(new BankResponse(true, refId));
    }
}