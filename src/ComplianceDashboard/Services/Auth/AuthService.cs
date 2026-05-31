using ComplianceDashboard.Contracts.Auth;
using ComplianceDashboard.Data;
using ComplianceDashboard.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceDashboard.Services.Auth;

public class AuthService(
    DashboardDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<ServiceResult<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            return ServiceResult<AuthResponse>.Failure(ErrorCodes.ValidationError, "fullName is required.");

        if (string.IsNullOrWhiteSpace(request.Phone))
            return ServiceResult<AuthResponse>.Failure(ErrorCodes.ValidationError, "phone is required.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return ServiceResult<AuthResponse>.Failure(ErrorCodes.ValidationError,
                "password must be at least 8 characters.");

        var phone = request.Phone.Trim();
        var exists = await dbContext.Users.AnyAsync(user => user.Phone == phone, cancellationToken);
        if (exists)
            return ServiceResult<AuthResponse>.Failure(ErrorCodes.ValidationError,
                "User with this phone already exists.");

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Phone = phone,
            ApiKey = $"dashboard-{Guid.NewGuid():N}",
            SecretKeyHash = Guid.NewGuid().ToString("N"),
            PasswordHash = passwordHasher.Hash(request.Password),
            AccountStatus = "ACTIVE",
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<AuthResponse>.Success(CreateAuthResponse(user));
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var phone = request.Phone.Trim();
        var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Phone == phone, cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return ServiceResult<AuthResponse>.Failure(ErrorCodes.ValidationError, "Invalid phone or password.");

        return ServiceResult<AuthResponse>.Success(CreateAuthResponse(user));
    }

    public async Task<ServiceResult<AuthUserResponse>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
        return user is null
            ? ServiceResult<AuthUserResponse>.Failure(ErrorCodes.NotFound, "User not found.")
            : ServiceResult<AuthUserResponse>.Success(ToUserResponse(user));
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var token = jwtTokenService.CreateToken(user);
        return new AuthResponse(token.Token, token.ExpiresAt, ToUserResponse(user));
    }

    private static AuthUserResponse ToUserResponse(User user)
    {
        return new AuthUserResponse(
            user.Id.ToString(),
            user.FullName,
            user.Phone,
            user.AccountStatus);
    }
}