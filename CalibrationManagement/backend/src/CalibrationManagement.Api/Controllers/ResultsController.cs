using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CalibrationManagement.Api.Authorization;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Api.Controllers;

[Route("api/results")]
[Authorize(Policy = PolicyNames.Read)]
public sealed class ResultsController : ApiControllerBase
{
    private readonly IResultService _service;

    public ResultsController(IResultService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ResultDto>>> Search([FromQuery] ResultQuery query, CancellationToken ct)
        => Ok(await _service.SearchAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ResultDto>> Get(int id, CancellationToken ct) => Ok(await _service.GetAsync(id, ct));

    [HttpPost]
    [Authorize(Policy = PolicyNames.Write)]
    public async Task<ActionResult<ResultDto>> Create([FromBody] ResultRequest request, CancellationToken ct)
    {
        var dto = await _service.CreateAsync(request, CurrentUserId, ct);
        return CreatedAtAction(nameof(Get), new { id = dto.ResultId }, dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyNames.Write)]
    public async Task<ActionResult<ResultDto>> Update(int id, [FromBody] ResultRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, CurrentUserId, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PolicyNames.Manage)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, CurrentUserId, ct);
        return NoContent();
    }
}
