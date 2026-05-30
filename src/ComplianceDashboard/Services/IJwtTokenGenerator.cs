using ComplianceDashboard.Entities;

namespace ComplianceDashboard.Services;

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateToken(ApplicationUser user, IEnumerable<string>? roles = null);
}