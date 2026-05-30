namespace GatewayApi.Services.Fraud;

public class FraudOptions
{
    public string ScoreUrl { get; set; } = "https://web-production-915dc.up.railway.app/score";

    public string InterpretUrl { get; set; } = "https://riskscore-production.up.railway.app/interpret";

    public int TimeoutSeconds { get; set; } = 10;
}