using System.Text.Json;
using System.Text.Json.Serialization;

namespace GatewayApi.Services.Fraud;

public enum FraudRiskLevel
{
    None,
    Low,
    Medium,
    High
}

public sealed record FraudScoringInput(
    int Step,
    string Type,
    decimal Amount,
    decimal OldbalanceOrg,
    decimal NewbalanceOrig,
    decimal OldbalanceDest,
    decimal NewbalanceDest,
    string NameOrig,
    string NameDest);

public sealed class FraudScoreRequest
{
    [JsonPropertyName("step")] public int Step { get; init; }

    [JsonPropertyName("type")] public string Type { get; init; } = string.Empty;

    [JsonPropertyName("amount")] public decimal Amount { get; init; }

    [JsonPropertyName("oldbalanceOrg")] public decimal OldbalanceOrg { get; init; }

    [JsonPropertyName("newbalanceOrig")] public decimal NewbalanceOrig { get; init; }

    [JsonPropertyName("oldbalanceDest")] public decimal OldbalanceDest { get; init; }

    [JsonPropertyName("newbalanceDest")] public decimal NewbalanceDest { get; init; }

    [JsonPropertyName("nameOrig")] public string NameOrig { get; init; } = string.Empty;

    [JsonPropertyName("nameDest")] public string NameDest { get; init; } = string.Empty;
}

public sealed class FraudScoreResponse
{
    [JsonPropertyName("fraud_score")] public decimal FraudScore { get; init; }

    [JsonPropertyName("risk_tier")] public string RiskTier { get; init; } = string.Empty;

    [JsonPropertyName("is_fraud")] public bool IsFraud { get; init; }

    [JsonPropertyName("signals")] public JsonElement Signals { get; init; }

    [JsonPropertyName("evaluated_at")] public DateTimeOffset EvaluatedAt { get; init; }
}

public sealed class FraudInterpretationResponse
{
    [JsonPropertyName("status")] public string Status { get; init; } = string.Empty;

    [JsonPropertyName("risk_tier")] public string RiskTier { get; init; } = string.Empty;

    [JsonPropertyName("summary")] public string Summary { get; init; } = string.Empty;

    [JsonPropertyName("reasons")] public IReadOnlyCollection<string> Reasons { get; init; } = [];

    [JsonPropertyName("advice")] public string Advice { get; init; } = string.Empty;
}

public sealed record FraudEvaluationResult(
    bool IsAvailable,
    string? Error,
    FraudScoreResponse? Score,
    FraudInterpretationResponse? Interpretation)
{
    public bool IsFraud => Score?.IsFraud == true;

    public FraudRiskLevel RiskLevel => FraudRiskTierParser.Parse(Score?.RiskTier);
}

public static class FraudRiskTierParser
{
    public static FraudRiskLevel Parse(string? riskTier)
    {
        if (string.IsNullOrWhiteSpace(riskTier)) return FraudRiskLevel.None;

        var normalized = riskTier.Trim().ToLowerInvariant();
        if (normalized.Contains("high")) return FraudRiskLevel.High;
        if (normalized.Contains("medium")) return FraudRiskLevel.Medium;
        if (normalized.Contains("low")) return FraudRiskLevel.Low;

        return FraudRiskLevel.None;
    }
}