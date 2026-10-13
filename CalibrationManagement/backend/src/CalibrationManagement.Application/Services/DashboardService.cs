using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Application.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repo;
    private readonly INotificationService _notifications;

    public DashboardService(IDashboardRepository repo, INotificationService notifications)
    {
        _repo = repo;
        _notifications = notifications;
    }

    public async Task<DashboardDto> GetAsync(int userId, CancellationToken ct = default)
    {
        await _notifications.EnsureFreshAsync(ct);
        return await _repo.GetSummaryAsync(userId, ct);
    }
}
