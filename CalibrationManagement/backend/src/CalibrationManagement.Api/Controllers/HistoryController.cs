using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CalibrationManagement.Api.Authorization;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Api.Controllers;

[Route("api/history")]
[Authorize(Policy = PolicyNames.Read)]
public sealed class HistoryController : ApiControllerBase
{
    private readonly IResultService _service;

    public HistoryController(IResultService service) => _service = service;

    /// <summary>Equipment history card (QA.FMT-29): equipment header plus every calibration row.</summary>
    [HttpGet("{equipmentId:int}")]
    public async Task<ActionResult<HistoryCardDto>> Get(int equipmentId, CancellationToken ct) => Ok(await _service.GetHistoryAsync(equipmentId, ct));
}
