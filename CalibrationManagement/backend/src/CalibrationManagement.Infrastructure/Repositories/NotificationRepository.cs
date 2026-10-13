using Dapper;
using Microsoft.Data.SqlClient;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Infrastructure.Data;

namespace CalibrationManagement.Infrastructure.Repositories;

public sealed class NotificationRepository : BaseRepository, INotificationRepository
{
    public NotificationRepository(IDbConnectionFactory factory) : base(factory) { }

    public async Task<int> GenerateAsync(CancellationToken ct = default)
    {
        try
        {
            await using var connection = Factory.CreateConnection();
            return await connection.QuerySingleAsync<int>(MultiCmd("dbo.usp_Notifications_Generate", null, ct));
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return 0; // another request generated the same notifications concurrently
        }
    }

    public Task<PagedResult<NotificationDto>> ListAsync(int userId, NotificationQuery q, CancellationToken ct = default)
        => QueryPagedAsync<NotificationDto>("dbo.usp_Notifications_List",
            new { UserId = userId, q.UnreadOnly, Page = q.SafePage, PageSize = q.SafePageSize }, q.SafePage, q.SafePageSize, ct);

    public Task<int> UnreadCountAsync(int userId, CancellationToken ct = default)
        => QuerySingleAsync<int>("dbo.usp_Notifications_UnreadCount", new { UserId = userId }, ct);

    public Task MarkReadAsync(int userId, int notificationId, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Notifications_MarkRead", new { UserId = userId, NotificationId = notificationId }, ct);

    public Task<int> MarkAllReadAsync(int userId, CancellationToken ct = default)
        => QuerySingleAsync<int>("dbo.usp_Notifications_MarkAllRead", new { UserId = userId }, ct);
}
