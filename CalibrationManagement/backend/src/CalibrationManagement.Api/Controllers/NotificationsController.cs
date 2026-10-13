using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CalibrationManagement.Api.Authorization;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Api.Controllers;

[Route("api/notifications")]
[Authorize(Policy = PolicyNames.Read)]
public sealed class NotificationsController : ApiControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationDto>>> List([FromQuery] NotificationQuery query, CancellationToken ct)
        => Ok(await _service.ListAsync(CurrentUserId, query, ct));

    [HttpGet("unread-count")]
    public async Task<ActionResult<object>> UnreadCount(CancellationToken ct)
        => Ok(new { unreadCount = await _service.UnreadCountAsync(CurrentUserId, ct) });

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
    {
        await _service.MarkReadAsync(CurrentUserId, id, ct);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<object>> MarkAllRead(CancellationToken ct)
        => Ok(new { updated = await _service.MarkAllReadAsync(CurrentUserId, ct) });

    /// <summary>Re-evaluates due / overdue / failed calibrations and creates any missing notifications.</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<object>> Refresh(CancellationToken ct)
        => Ok(new { created = await _service.RefreshAsync(ct) });
}
