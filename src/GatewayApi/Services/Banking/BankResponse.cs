namespace GatewayApi.Services.Banking;

public record BankResponse(
    bool Success,
    string BankReferenceId,
    string? FailureReason = null,
    int LatencyMs = 0);