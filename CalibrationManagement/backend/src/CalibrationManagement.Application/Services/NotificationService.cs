using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Application.Services;

public sealed class NotificationService : INotificationService
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);
    private static DateTime _lastRefreshUtc = DateTime.MinValue;
    private static readonly object RefreshLock = new();

    private readonly INotificationRepository _repo;

    public NotificationService(INotificationRepository repo) => _repo = repo;

    public async Task EnsureFreshAsync(CancellationToken ct = default)
    {
        lock (RefreshLock)
        {
            if (DateTime.UtcNow - _lastRefreshUtc < RefreshInterval) return;
            _lastRefreshUtc = DateTime.UtcNow;
        }
        await _repo.GenerateAsync(ct);
    }

    public async Task<int> RefreshAsync(CancellationToken ct = default)
    {
        lock (RefreshLock) { _lastRefreshUtc = DateTime.UtcNow; }
        return await _repo.GenerateAsync(ct);
    }

    public async Task<PagedResult<NotificationDto>> ListAsync(int userId, NotificationQuery query, CancellationToken ct = default)
    {
        await EnsureFreshAsync(ct);
        return await _repo.ListAsync(userId, query, ct);
    }

    public async Task<int> UnreadCountAsync(int userId, CancellationToken ct = default)
    {
        await EnsureFreshAsync(ct);
        return await _repo.UnreadCountAsync(userId, ct);
    }

    public Task MarkReadAsync(int userId, int notificationId, CancellationToken ct = default) => _repo.MarkReadAsync(userId, notificationId, ct);

    public Task<int> MarkAllReadAsync(int userId, CancellationToken ct = default) => _repo.MarkAllReadAsync(userId, ct);
}
