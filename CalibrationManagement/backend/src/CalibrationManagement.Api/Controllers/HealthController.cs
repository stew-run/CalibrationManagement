using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Api.Controllers;

[Route("api/health")]
public sealed class HealthController : ApiControllerBase
{
    private readonly ILookupService _lookups;

    public HealthController(ILookupService lookups) => _lookups = lookups;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var dbOk = await _lookups.CanConnectAsync(ct);
        var body = new { status = dbOk ? "ok" : "degraded", database = dbOk ? "ok" : "unavailable", timeUtc = DateTime.UtcNow };
        return dbOk ? Ok(body) : StatusCode(StatusCodes.Status503ServiceUnavailable, body);
    }
}
