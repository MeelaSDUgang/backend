namespace GatewayApi.Services.Banking.Adapters;

public class HalykBankAdapter : IPaymentAdapter
{
    public string RoutingKey => "halykP2P";

    public Task<BankResponse> ProcessPaymentAsync(PaymentContext context, CancellationToken ct = default)
    {
        var refId = $"Halyk-{context.Type}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        return Task.FromResult(new BankResponse(true, refId));
    }
}