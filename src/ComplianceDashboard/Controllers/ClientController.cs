using ComplianceDashboard.Contracts;
using ComplianceDashboard.Contracts.Appeals;
using ComplianceDashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceDashboard.Controllers;

[Authorize]
[Route("api")]
[Tags("Client")]
public class ClientController(IClientAppealService clientAppealService) : ApiControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        return FromServiceResult(await clientAppealService.GetCurrentUserAsync(userId, cancellationToken));
    }

    [HttpGet("operations/blocked")]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationResponse>> GetBlockedOperation(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        return FromServiceResult(await clientAppealService.GetBlockedOperationAsync(userId, cancellationToken));
    }

    [HttpGet("operations/blocked/all")]
    [ProducesResponseType<IReadOnlyCollection<OperationResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<OperationResponse>>> GetBlockedOperations(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        return Ok(await clientAppealService.GetBlockedOperationsAsync(userId, cancellationToken));
    }
}
