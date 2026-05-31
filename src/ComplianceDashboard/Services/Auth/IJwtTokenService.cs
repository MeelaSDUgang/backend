using ComplianceDashboard.Entities;

namespace ComplianceDashboard.Services.Auth;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateToken(User user);
}