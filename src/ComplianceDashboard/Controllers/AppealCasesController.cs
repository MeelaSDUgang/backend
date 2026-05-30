using ComplianceDashboard.Contracts;
using ComplianceDashboard.Contracts.Appeals;
using ComplianceDashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceDashboard.Controllers;

[Route("api/appeal-cases")]
[Tags("Client Appeal Flow")]
public class AppealCasesController(IClientAppealService clientAppealService) : ApiControllerBase
{
    /// <summary>
    ///     Creates a draft appeal case.
    /// </summary>
    /// <remarks>
    ///     For OPERATION_CONFIRMATION, operationId is required and userId is taken from the operation.
    ///     Default status is DRAFT; default route is SUPPORT.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType<AppealCaseResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppealCaseResponse>> Create(
        CreateAppealCaseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await clientAppealService.CreateAppealCaseAsync(request, cancellationToken);
        if (!result.Succeeded) return ErrorResult(result);

        return CreatedAtAction(nameof(Get), new { caseId = result.Value!.Id }, result.Value);
    }

    /// <summary>
    ///     Returns a full appeal case with user, operation, answers, documents, decisions, and parsed missingInfo.
    /// </summary>
    [HttpGet("{caseId}")]
    [ProducesResponseType<AppealCaseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppealCaseResponse>> Get(
        string caseId,
        CancellationToken cancellationToken)
    {
        return FromServiceResult(await clientAppealService.GetAppealCaseAsync(caseId, cancellationToken));
    }

    /// <summary>
    ///     Replaces previous answers for the draft case and saves the provided answers.
    /// </summary>
    [HttpPost("{caseId}/answers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> SaveAnswers(
        string caseId,
        SaveAppealAnswersRequest request,
        CancellationToken cancellationToken)
    {
        return FromServiceResult(await clientAppealService.SaveAnswersAsync(caseId, request, cancellationToken));
    }

    /// <summary>
    ///     Adds a mock document record to a draft appeal case.
    /// </summary>
    /// <remarks>
    ///     No multipart upload is required. The API stores fileName, documentType, and sets mockUrl to /mock-files/{fileName}.
    /// </remarks>
    [HttpPost("{caseId}/documents")]
    [ProducesResponseType<AppealDocumentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppealDocumentResponse>> AddDocument(
        string caseId,
        AddAppealDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await clientAppealService.AddDocumentAsync(caseId, request, cancellationToken);
        if (!result.Succeeded) return ErrorResult(result);

        return Created($"/api/appeal-cases/{caseId}/documents/{result.Value!.Id}", result.Value);
    }

    /// <summary>
    ///     Generates deterministic support summary and submits the case.
    /// </summary>
    /// <remarks>
    ///     This is not a fraud decision and does not unblock anything.
    /// </remarks>
    [HttpPost("{caseId}/generate-summary")]
    [ProducesResponseType<GenerateSupportSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenerateSupportSummaryResponse>> GenerateSummary(
        string caseId,
        CancellationToken cancellationToken)
    {
        return FromServiceResult(await clientAppealService.GenerateSupportSummaryAsync(caseId, cancellationToken));
    }
}