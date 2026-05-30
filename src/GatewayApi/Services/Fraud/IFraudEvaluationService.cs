namespace GatewayApi.Services.Fraud;

public interface IFraudEvaluationService
{
    Task<FraudEvaluationResult> EvaluateAsync(FraudScoringInput input, CancellationToken cancellationToken);
}