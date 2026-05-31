namespace ComplianceDashboard.Services.Auth;

public class JwtOptions
{
    public string Issuer { get; set; } = "ComplianceDashboard";

    public string Audience { get; set; } = "ComplianceDashboard";

    public string SigningKey { get; set; } = "dev-only-compliance-dashboard-signing-key-change-me";

    public int ExpiresMinutes { get; set; } = 120;
}