using GatewayApi.Data;
using GatewayApi.Filters;
using GatewayApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[ServiceFilter(typeof(HmacAuthFilter))]
public class GatewaysController : ControllerBase
{
    private readonly AppDbContext _db;

    public GatewaysController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GatewayResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGateways(CancellationToken ct)
    {
        var gateways = await _db.BankAdapters
            .AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .Select(b => new GatewayResponse(
                b.Id,
                b.Name,
                b.IsActive,
                b.SupportedGatewayTypes.Split(',', StringSplitOptions.RemoveEmptyEntries)))
            .ToListAsync(ct);

        return Ok(gateways);
    }
}