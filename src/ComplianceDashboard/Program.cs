using System.Reflection;
using ComplianceDashboard.Contracts;
using ComplianceDashboard.Contracts.Appeals;
using ComplianceDashboard.Contracts.Support;
using ComplianceDashboard.Data;
using ComplianceDashboard.Services;
using ComplianceDashboard.Services.TransactionProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DashboardDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IClientAppealService, ClientAppealService>();
builder.Services.AddScoped<ISupportService, SupportService>();
builder.Services.AddSingleton<ITransactionWorkerQueue, TransactionWorkerQueue>();
builder.Services.AddScoped<ILowRiskPollingService, LowRiskPollingService>();
builder.Services.AddScoped<IHighRiskVerificationService, HighRiskVerificationService>();
builder.Services.AddHostedService<TransactionReviewWorker>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Compliance Dashboard API",
        Version = "v1",
        Description = """
                      Жай демо
                      """
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Compliance Dashboard API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Compliance Dashboard API";
});

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.MapGet("/api/health", () => new { status = "ok" })
    .WithTags("Health")
    .WithSummary("Health check")
    .WithDescription("Returns a simple status payload to verify that the API process is running.")
    .Produces(StatusCodes.Status200OK);

app.MapGet("/api/me", async (
            IClientAppealService service,
            CancellationToken cancellationToken) =>
        ToResult(await service.GetCurrentUserAsync(cancellationToken)))
    .WithTags("Client")
    .WithSummary("Get current demo user")
    .WithDescription("Returns seeded user `user_1`. No authentication is required for this MVP.")
    .Produces<UserResponse>()
    .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

app.MapGet("/api/operations/blocked", async (
            IClientAppealService service,
            CancellationToken cancellationToken) =>
        ToResult(await service.GetBlockedOperationAsync(cancellationToken)))
    .WithTags("Client")
    .WithSummary("Get latest blocked operation")
    .WithDescription("Returns the latest `PENDING_CONFIRMATION` or `BLOCKED` operation for demo user `user_1`.")
    .Produces<OperationResponse>()
    .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

app.MapPost("/api/appeal-cases", async (
        CreateAppealCaseRequest request,
        IClientAppealService service,
        CancellationToken cancellationToken) =>
    {
        var result = await service.CreateAppealCaseAsync(request, cancellationToken);
        return result.Succeeded
            ? Results.Created($"/api/appeal-cases/{result.Value!.Id}", result.Value)
            : ToErrorResult(result);
    })
    .WithTags("Client Appeal Flow")
    .WithSummary("Create appeal case")
    .WithDescription("""
                     Creates a draft appeal case.

                     For `OPERATION_CONFIRMATION`, `operationId` is required and `userId` is taken from the operation.
                     Default status is `DRAFT`; default route is `SUPPORT`.
                     """)
    .Accepts<CreateAppealCaseRequest>("application/json")
    .Produces<AppealCaseResponse>(StatusCodes.Status201Created)
    .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
    .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

app.MapGet("/api/appeal-cases/{caseId}", async (
            string caseId,
            IClientAppealService service,
            CancellationToken cancellationToken) =>
        ToResult(await service.GetAppealCaseAsync(caseId, cancellationToken)))
    .WithTags("Client Appeal Flow")
    .WithSummary("Get appeal case")
    .WithDescription(
        "Returns a full appeal case with user, operation, answers, documents, decisions, and parsed `missingInfo`.")
    .Produces<AppealCaseResponse>()
    .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

app.MapPost("/api/appeal-cases/{caseId}/answers", async (
            string caseId,
            SaveAppealAnswersRequest request,
            IClientAppealService service,
            CancellationToken cancellationToken) =>
        ToResult(await service.SaveAnswersAsync(caseId, request, cancellationToken)))
    .WithTags("Client Appeal Flow")
    .WithSummary("Save appeal answers")
    .WithDescription("""
                     Replaces previous answers for the draft case and saves the provided answers.

                     The case must still be `DRAFT`. Each answer requires `questionKey`, `questionText`, and `answer`.
                     """)
    .Accepts<SaveAppealAnswersRequest>("application/json")
    .Produces(StatusCodes.Status200OK)
    .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
    .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

app.MapPost("/api/appeal-cases/{caseId}/documents", async (
        string caseId,
        AddAppealDocumentRequest request,
        IClientAppealService service,
        CancellationToken cancellationToken) =>
    {
        var result = await service.AddDocumentAsync(caseId, request, cancellationToken);
        return result.Succeeded
            ? Results.Created($"/api/appeal-cases/{caseId}/documents/{result.Value!.Id}", result.Value)
            : ToErrorResult(result);
    })
    .WithTags("Client Appeal Flow")
    .WithSummary("Add mock appeal document")
    .WithDescription("""
                     Adds a document record to a draft appeal case.

                     No multipart upload is required for MVP. The API stores `fileName`, `documentType`, and sets `mockUrl` to `/mock-files/{fileName}`.
                     """)
    .Accepts<AddAppealDocumentRequest>("application/json")
    .Produces<AppealDocumentResponse>(StatusCodes.Status201Created)
    .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
    .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

app.MapPost("/api/appeal-cases/{caseId}/generate-summary", async (
            string caseId,
            IClientAppealService service,
            CancellationToken cancellationToken) =>
        ToResult(await service.GenerateSupportSummaryAsync(caseId, cancellationToken)))
    .WithTags("Client Appeal Flow")
    .WithSummary("Generate support summary")
    .WithDescription("""
                     Runs deterministic rule-based summary generation for support.

                     This is not a fraud decision and does not unblock anything. It fills `supportSummary`, `clientMessage`, `missingInfoJson`, sets route, and moves the case to `SUBMITTED`.
                     """)
    .Produces<GenerateSupportSummaryResponse>()
    .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
    .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

app.MapGet("/api/support/appeal-cases", async (
            ISupportService service,
            CancellationToken cancellationToken) =>
        Results.Ok(await service.GetSupportCasesAsync(cancellationToken)))
    .WithTags("Support Dashboard")
    .WithSummary("Get support-visible appeal cases")
    .WithDescription(
        "Returns cases with statuses `SUBMITTED`, `WAITING_SUPPORT`, `NEED_MORE_INFO`, `RESOLVED`, or `ESCALATED` for dashboard list view.")
    .Produces<IReadOnlyCollection<SupportCaseListItemResponse>>();

app.MapGet("/api/support/appeal-cases/{caseId}", async (
            string caseId,
            ISupportService service,
            CancellationToken cancellationToken) =>
        ToResult(await service.GetSupportCaseAsync(caseId, cancellationToken)))
    .WithTags("Support Dashboard")
    .WithSummary("Get support case details")
    .WithDescription(
        "Returns full support detail data: client, operation, answers, documents, summary, missing info, client message, and decisions.")
    .Produces<SupportCaseDetailsResponse>()
    .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

app.MapPost("/api/support/appeal-cases/{caseId}/decision", async (
            string caseId,
            SubmitSupportDecisionRequest request,
            ISupportService service,
            CancellationToken cancellationToken) =>
        ToResult(await service.SubmitDecisionAsync(caseId, request, cancellationToken)))
    .WithTags("Support Dashboard")
    .WithSummary("Submit support decision")
    .WithDescription("""
                     Records a demo support decision and updates case status.

                     `CONFIRM_OPERATION` -> case `RESOLVED`, operation `SUCCESS`.
                     `REQUEST_MORE_INFO` -> case `NEED_MORE_INFO`.
                     `KEEP_BLOCKED` -> case `RESOLVED`, operation `BLOCKED`.
                     `ESCALATE` -> case `ESCALATED`.

                     This is demo behavior only, not a real bank unblock.
                     """)
    .Accepts<SubmitSupportDecisionRequest>("application/json")
    .Produces<SubmitSupportDecisionResponse>()
    .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
    .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

app.Run();

static IResult ToResult<T>(ServiceResult<T> result)
{
    return result.Succeeded ? Results.Ok(result.Value) : ToErrorResult(result);
}

static IResult ToErrorResult<T>(ServiceResult<T> result)
{
    var response = new ApiErrorResponse(
        result.Error ?? ErrorCodes.InternalError,
        result.Message ?? "Internal error.");

    return response.Error switch
    {
        ErrorCodes.NotFound => Results.NotFound(response),
        ErrorCodes.ValidationError => Results.BadRequest(response),
        ErrorCodes.CaseAlreadySubmitted => Results.BadRequest(response),
        _ => Results.Json(response, statusCode: StatusCodes.Status500InternalServerError)
    };
}