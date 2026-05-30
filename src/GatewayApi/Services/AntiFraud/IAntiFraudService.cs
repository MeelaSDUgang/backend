namespace GatewayApi.Services.AntiFraud;

public interface IAntiFraudService
{
    Task<FraudCheckResult> EvaluateAsync(FraudEvaluationContext context, CancellationToken ct = default);
}

public record FraudEvaluationContext(
    decimal Amount,
    string Currency,
    string GatewayType,
    string MerchantName,
    DateTime MerchantCreatedAt,
    string AccountIdentifier,
    string RawPayload);