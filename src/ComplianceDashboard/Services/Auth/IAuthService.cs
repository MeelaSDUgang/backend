using ComplianceDashboard.Contracts.Auth;

namespace ComplianceDashboard.Services.Auth;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<ServiceResult<AuthUserResponse>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}