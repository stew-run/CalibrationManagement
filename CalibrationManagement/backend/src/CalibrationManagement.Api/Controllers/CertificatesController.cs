using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CalibrationManagement.Api.Authorization;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Api.Controllers;

[Route("api/certificates")]
[Authorize(Policy = PolicyNames.Read)]
public sealed class CertificatesController : ApiControllerBase
{
    private readonly ICertificateService _service;

    public CertificatesController(ICertificateService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<CertificateDto>>> Search([FromQuery] CertificateQuery query, CancellationToken ct)
        => Ok(await _service.SearchAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CertificateDto>> Get(int id, CancellationToken ct) => Ok(await _service.GetAsync(id, ct));

    [HttpPost]
    [Authorize(Policy = PolicyNames.Write)]
    public async Task<ActionResult<CertificateDto>> Create([FromBody] CertificateRequest request, CancellationToken ct)
    {
        var dto = await _service.CreateAsync(request, CurrentUserId, ct);
        return CreatedAtAction(nameof(Get), new { id = dto.CertificateId }, dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PolicyNames.Write)]
    public async Task<ActionResult<CertificateDto>> Update(int id, [FromBody] CertificateRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, CurrentUserId, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PolicyNames.Manage)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, CurrentUserId, ct);
        return NoContent();
    }
}
