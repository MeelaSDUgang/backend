namespace GatewayApi.Services.Banking;

public interface IPaymentAdapter
{
    string RoutingKey { get; }
    Task<BankResponse> ProcessPaymentAsync(PaymentContext context, CancellationToken ct = default);
}