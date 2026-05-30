using GatewayApi.Data;
using GatewayApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Filters;

public class HmacAuthFilter(AppDbContext db) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;

        if (!request.Headers.TryGetValue("X-API-Key", out var apiKey) ||
            !request.Headers.TryGetValue("X-Signature", out var signature) ||
            !request.Headers.TryGetValue("X-Timestamp", out var timestampStr))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Missing security headers" });
            return;
        }

        if (!long.TryParse(timestampStr, out var timestamp))
        {
            context.Result = new BadRequestObjectResult(new { error = "Invalid timestamp" });
            return;
        }

        var requestTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        if (Math.Abs(DateTimeOffset.UtcNow.Subtract(requestTime).TotalMinutes) > 5)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Request expired" });
            return;
        }

        request.EnableBuffering();

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        request.Body.Position = 0;

        var merchant = await db.Merchants
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ApiKey == apiKey.ToString());

        if (merchant == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var serverSignature = SecurityHelper.ComputeHmacSignature($"{body}{timestampStr}", merchant.SecretKeyHash);

        if (serverSignature != signature)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Invalid signature" });
            return;
        }

        context.HttpContext.Items["Merchant"] = merchant;
    }
}