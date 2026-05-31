using ComplianceDashboard.Contracts;
using ComplianceDashboard.Contracts.Support;
using ComplianceDashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceDashboard.Controllers;

[Authorize]
[Route("api/support/appeal-cases")]
[Tags("Support Dashboard")]
public class SupportAppealCasesController(ISupportService supportService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<SupportCaseListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<SupportCaseListItemResponse>>> GetCases(
        CancellationToken cancellationToken)
    {
        return Ok(await supportService.GetSupportCasesAsync(cancellationToken));
    }

    [HttpGet("{caseId}")]
    [ProducesResponseType<SupportCaseDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupportCaseDetailsResponse>> GetCase(
        string caseId,
        CancellationToken cancellationToken)
    {
        return FromServiceResult(await supportService.GetSupportCaseAsync(caseId, cancellationToken));
    }

    [HttpPost("{caseId}/decision")]
    [ProducesResponseType<SubmitSupportDecisionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubmitSupportDecisionResponse>> SubmitDecision(
        string caseId,
        SubmitSupportDecisionRequest request,
        CancellationToken cancellationToken)
    {
        return FromServiceResult(await supportService.SubmitDecisionAsync(caseId, request, cancellationToken));
    }
}