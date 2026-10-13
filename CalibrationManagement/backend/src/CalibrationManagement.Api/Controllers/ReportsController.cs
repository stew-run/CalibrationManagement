using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CalibrationManagement.Api.Authorization;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Api.Controllers;

[Route("api/reports")]
[Authorize(Policy = PolicyNames.Read)]
public sealed class ReportsController : ApiControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service) => _service = service;

    [HttpGet("calibration")]
    public async Task<ActionResult<PagedResult<CalibrationReportRowDto>>> Calibration([FromQuery] CalibrationReportQuery query, CancellationToken ct)
        => Ok(await _service.CalibrationAsync(query, ct));

    [HttpGet("calibration/export")]
    public async Task<IActionResult> ExportCalibration([FromQuery] CalibrationReportQuery query, CancellationToken ct)
    {
        var bytes = await _service.ExportCalibrationCsvAsync(query, ct);
        var name = $"calibration-report-{query.ReportType}-{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(bytes, "text/csv; charset=utf-8", name);
    }

    [HttpGet("agency-summary")]
    public async Task<ActionResult<IReadOnlyList<AgencySummaryDto>>> AgencySummary([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await _service.AgencySummaryAsync(from, to, ct));

    [HttpGet("data-quality")]
    [Authorize(Policy = PolicyNames.Manage)]
    public async Task<ActionResult<IReadOnlyList<DataQualityIssueDto>>> DataQuality([FromQuery] bool onlyOpen = true, CancellationToken ct = default)
        => Ok(await _service.DataQualityAsync(onlyOpen, ct));

    [HttpPost("data-quality/{id:int}/resolve")]
    [Authorize(Policy = PolicyNames.Manage)]
    public async Task<IActionResult> ResolveDataQuality(int id, [FromBody] ResolveIssueRequest request, CancellationToken ct)
    {
        await _service.ResolveDataQualityAsync(id, request.IsResolved, CurrentUserId, ct);
        return NoContent();
    }
}
