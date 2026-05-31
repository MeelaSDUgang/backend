using ComplianceDashboard.Contracts;
using ComplianceDashboard.Contracts.Appeals;
using ComplianceDashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceDashboard.Controllers;

[Authorize]
[Route("api/appeal-cases")]
[Tags("Client Appeal Flow")]
public class AppealCasesController(IClientAppealService clientAppealService) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<AppealCaseResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppealCaseResponse>> Create(
        CreateAppealCaseRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var result = await clientAppealService.CreateAppealCaseAsync(userId, request, cancellationToken);
        if (!result.Succeeded) return ErrorResult(result);

        return CreatedAtAction(nameof(Get), new { caseId = result.Value!.Id }, result.Value);
    }

    [HttpGet("{caseId}")]
    [ProducesResponseType<AppealCaseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppealCaseResponse>> Get(
        string caseId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        return FromServiceResult(await clientAppealService.GetAppealCaseAsync(userId, caseId, cancellationToken));
    }

    [HttpPost("{caseId}/answers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> SaveAnswers(
        string caseId,
        SaveAppealAnswersRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        return FromServiceResult(
            await clientAppealService.SaveAnswersAsync(userId, caseId, request, cancellationToken));
    }

    [HttpPost("{caseId}/documents")]
    [ProducesResponseType<AppealDocumentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppealDocumentResponse>> AddDocument(
        string caseId,
        AddAppealDocumentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var result = await clientAppealService.AddDocumentAsync(userId, caseId, request, cancellationToken);
        if (!result.Succeeded) return ErrorResult(result);

        return Created($"/api/appeal-cases/{caseId}/documents/{result.Value!.Id}", result.Value);
    }

    [HttpPost("{caseId}/generate-summary")]
    [ProducesResponseType<GenerateSupportSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenerateSupportSummaryResponse>> GenerateSummary(
        string caseId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        return FromServiceResult(
            await clientAppealService.GenerateSupportSummaryAsync(userId, caseId, cancellationToken));
    }
}