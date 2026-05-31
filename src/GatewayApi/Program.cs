using System.Reflection;
using GatewayApi.Data;
using GatewayApi.Filters;
using GatewayApi.Services;
using GatewayApi.Services.Banking;
using GatewayApi.Services.Banking.Adapters;
using GatewayApi.Services.Fraud;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<HmacAuthFilter>();

builder.Services.AddSingleton<IPaymentAdapter, HalykBankAdapter>();

builder.Services.AddScoped<PaymentOrchestrator>();
builder.Services.AddScoped<IdempotencyService>();
builder.Services.Configure<FraudOptions>(builder.Configuration.GetSection("Fraud"));
builder.Services.AddHttpClient<IFraudEvaluationService, FraudEvaluationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payment Gateway API",
        Version = "v1",
        Description = """
                      Payment gateway endpoints.

                      HMAC auth:
                      - X-API-Key: user API key
                      - X-Timestamp: current Unix timestamp in seconds
                      - X-Signature: HMAC signature for raw request body + timestamp, signed by user's secret key

                      Payment creation also requires Idempotency-Key header with a UUID value.
                      """
    });

    options.AddSecurityDefinition("X-API-Key", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Description = "User API key."
    });
    options.AddSecurityDefinition("X-Signature", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "X-Signature",
        In = ParameterLocation.Header,
        Description = "HMAC signature for raw request body + X-Timestamp."
    });
    options.AddSecurityDefinition("X-Timestamp", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "X-Timestamp",
        In = ParameterLocation.Header,
        Description = "Unix timestamp in seconds. Requests expire after 5 minutes."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("X-API-Key", document)] = [],
        [new OpenApiSecuritySchemeReference("X-Signature", document)] = [],
        [new OpenApiSecuritySchemeReference("X-Timestamp", document)] = []
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<AppDbContext>();
if (context.Database.IsNpgsql()) context.Database.Migrate();
await DbInitializer.SeedAsync(context);

app.UseRouting();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Gateway API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Payment Gateway API";
});

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.MapControllers();

app.Run();