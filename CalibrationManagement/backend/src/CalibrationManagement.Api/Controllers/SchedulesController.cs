using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CalibrationManagement.Api.Authorization;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Api.Controllers;

[Route("api/schedules")]
[Authorize(Policy = PolicyNames.Read)]
public sealed class SchedulesController : ApiControllerBase
{
    private readonly IScheduleService _service;

    public SchedulesController(IScheduleService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ScheduleDto>>> Search([FromQuery] ScheduleQuery query, CancellationToken ct)
        => Ok(await _service.SearchAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ScheduleDto>> Get(int id, CancellationToken ct) => Ok(await _service.GetAsync(id, ct));

    [HttpPost]
    [Authorize(Policy = PolicyNames.Write)]
    public async Task<ActionResult<ScheduleDto>> Create([FromBody] CreateScheduleRequest request, CancellationToken ct)
    {
        var dto = await _service.CreateAsync(request, CurrentUserId, ct);
        return CreatedAtAction(nameof(Get), new { id = dto.ScheduleId }, dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyNames.Write)]
    public async Task<ActionResult<ScheduleDto>> Update(int id, [FromBody] UpdateScheduleRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, CurrentUserId, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PolicyNames.Manage)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, CurrentUserId, ct);
        return NoContent();
    }
}
