using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> ListAsync(int userId, NotificationQuery query, CancellationToken ct = default);
    Task<int> UnreadCountAsync(int userId, CancellationToken ct = default);
    Task<int> RefreshAsync(CancellationToken ct = default);
    Task EnsureFreshAsync(CancellationToken ct = default);
    Task MarkReadAsync(int userId, int notificationId, CancellationToken ct = default);
    Task<int> MarkAllReadAsync(int userId, CancellationToken ct = default);
}
