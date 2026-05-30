namespace GatewayApi.Services.AntiFraud;

/// <summary>
///     Result of AI-powered fraud analysis for a payment transaction.
/// </summary>
public record FraudCheckResult(
    /// <summary>Risk score from 0 (safe) to 100 (fraud).</summary>
    int RiskScore,
    /// <summary>Human-readable explanation of the risk assessment.</summary>
    string Reason,
    /// <summary>Whether the transaction should be blocked (score > 75).</summary>
    bool IsBlocked);