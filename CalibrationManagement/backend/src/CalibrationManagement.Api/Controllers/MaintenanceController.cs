using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CalibrationManagement.Api.Authorization;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Api.Controllers;

[Route("api/maintenance")]
[Authorize(Policy = PolicyNames.Read)]
public sealed class MaintenanceController : ApiControllerBase
{
    private readonly IMaintenanceService _service;

    public MaintenanceController(IMaintenanceService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<MaintenanceDto>>> Search([FromQuery] MaintenanceQuery query, CancellationToken ct)
        => Ok(await _service.SearchAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MaintenanceDto>> Get(int id, CancellationToken ct) => Ok(await _service.GetAsync(id, ct));

    [HttpPost]
    [Authorize(Policy = PolicyNames.Write)]
    public async Task<ActionResult<MaintenanceDto>> Create([FromBody] MaintenanceRequest request, CancellationToken ct)
    {
        var dto = await _service.CreateAsync(request, CurrentUserId, ct);
        return CreatedAtAction(nameof(Get), new { id = dto.MaintenanceId }, dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyNames.Write)]
    public async Task<ActionResult<MaintenanceDto>> Update(int id, [FromBody] MaintenanceRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, CurrentUserId, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PolicyNames.Manage)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, CurrentUserId, ct);
        return NoContent();
    }
}
