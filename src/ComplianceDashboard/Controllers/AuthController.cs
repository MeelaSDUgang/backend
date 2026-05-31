using System.Security.Claims;
using ComplianceDashboard.Contracts;
using ComplianceDashboard.Contracts.Auth;
using ComplianceDashboard.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceDashboard.Controllers;

[Route("api/auth")]
[Tags("Auth")]
public class AuthController(IAuthService authService) : ApiControllerBase
{
    /// <summary>
    ///     Creates a dashboard user and returns a JWT access token.
    /// </summary>
    /// <remarks>
    ///     No Identity and no roles are used. The token contains only user data claims.
    /// </remarks>
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        return FromServiceResult(await authService.RegisterAsync(request, cancellationToken));
    }

    /// <summary>
    ///     Authenticates by phone and password and returns a JWT access token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        return FromServiceResult(await authService.LoginAsync(request, cancellationToken));
    }

    /// <summary>
    ///     Returns user data from the authenticated JWT subject.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<AuthUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthUserResponse>> Me(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue("user_id");
        if (!Guid.TryParse(userId, out var parsedUserId)) return Unauthorized();

        return FromServiceResult(await authService.GetCurrentUserAsync(parsedUserId, cancellationToken));
    }
}