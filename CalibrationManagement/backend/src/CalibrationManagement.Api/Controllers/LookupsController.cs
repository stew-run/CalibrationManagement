using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CalibrationManagement.Api.Authorization;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Api.Controllers;

[Route("api/lookups")]
[Authorize(Policy = PolicyNames.Read)]
public sealed class LookupsController : ApiControllerBase
{
    private readonly ILookupService _lookups;

    public LookupsController(ILookupService lookups) => _lookups = lookups;

    [HttpGet]
    public async Task<ActionResult<LookupsDto>> Get(CancellationToken ct) => Ok(await _lookups.GetAllAsync(ct));

    [HttpPost("departments")]
    [Authorize(Policy = PolicyNames.Write)]
    public async Task<ActionResult<LookupItem>> CreateDepartment([FromBody] CreateLookupRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _lookups.CreateDepartmentAsync(request, CurrentUserId, ct));

    [HttpPost("agencies")]
    [Authorize(Policy = PolicyNames.Write)]
    public async Task<ActionResult<LookupItem>> CreateAgency([FromBody] CreateLookupRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _lookups.CreateAgencyAsync(request, CurrentUserId, ct));
}
