using ComplianceDashboard.Contracts;
using ComplianceDashboard.Contracts.Appeals;
using ComplianceDashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceDashboard.Controllers;

[Route("api")]
[Tags("Client")]
public class ClientController(IClientAppealService clientAppealService) : ApiControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        return FromServiceResult(await clientAppealService.GetCurrentUserAsync(cancellationToken));
    }

    [HttpGet("operations/blocked")]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationResponse>> GetBlockedOperation(CancellationToken cancellationToken)
    {
        return FromServiceResult(await clientAppealService.GetBlockedOperationAsync(cancellationToken));
    }
}