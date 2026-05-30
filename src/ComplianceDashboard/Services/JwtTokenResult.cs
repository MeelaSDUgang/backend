namespace ComplianceDashboard.Services;

public sealed record JwtTokenResult(string AccessToken, DateTimeOffset ExpiresAt);