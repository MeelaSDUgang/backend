using System.Security.Claims;
using ComplianceDashboard.Contracts;
using ComplianceDashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceDashboard.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected bool TryGetCurrentUserId(out Guid userId)
    {
        var claimValue = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claimValue, out userId);
    }

    protected ActionResult<T> FromServiceResult<T>(ServiceResult<T> result)
    {
        if (result.Succeeded) return Ok(result.Value);

        return ErrorResult(result);
    }

    protected ActionResult ErrorResult<T>(ServiceResult<T> result)
    {
        var response = new ApiErrorResponse(
            result.Error ?? ErrorCodes.InternalError,
            result.Message ?? "Internal error.");

        return response.Error switch
        {
            ErrorCodes.NotFound => NotFound(response),
            ErrorCodes.ValidationError => BadRequest(response),
            ErrorCodes.CaseAlreadySubmitted => BadRequest(response),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response)
        };
    }
}