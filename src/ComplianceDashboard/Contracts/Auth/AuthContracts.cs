namespace ComplianceDashboard.Contracts.Auth;

public sealed record RegisterRequest(string FullName, string Phone, string Password);

public sealed record LoginRequest(string Phone, string Password);

public sealed record AuthUserResponse(
    string Id,
    string FullName,
    string Phone,
    string AccountStatus);

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    AuthUserResponse User);