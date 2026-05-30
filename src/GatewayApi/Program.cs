using GatewayApi.Data;
using GatewayApi.Filters;
using GatewayApi.Services;
using GatewayApi.Services.Banking;
using GatewayApi.Services.Banking.Adapters;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<HmacAuthFilter>();

builder.Services.AddSingleton<IPaymentAdapter, HalykBankAdapter>();

builder.Services.AddScoped<PaymentOrchestrator>();
builder.Services.AddScoped<IdempotencyService>();

builder.Services.AddControllers();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<AppDbContext>();
if (context.Database.IsNpgsql()) context.Database.Migrate();
await DbInitializer.SeedAsync(context);

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();