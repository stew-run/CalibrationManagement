using Dapper;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Infrastructure.Data;

namespace CalibrationManagement.Infrastructure.Repositories;

public sealed class DashboardRepository : BaseRepository, IDashboardRepository
{
    public DashboardRepository(IDbConnectionFactory factory) : base(factory) { }

    public async Task<DashboardDto> GetSummaryAsync(int userId, CancellationToken ct = default)
    {
        await using var connection = Factory.CreateConnection();
        using var grid = await connection.QueryMultipleAsync(MultiCmd("dbo.usp_Dashboard_Summary", new { UserId = userId }, ct));
        return new DashboardDto
        {
            Metrics = await grid.ReadFirstAsync<DashboardMetricsDto>(),
            ByDepartment = (await grid.ReadAsync<DepartmentStatDto>()).AsList(),
            DueByMonth = (await grid.ReadAsync<MonthDueDto>()).AsList(),
            RecentResults = (await grid.ReadAsync<RecentResultDto>()).AsList(),
            Upcoming = (await grid.ReadAsync<UpcomingItemDto>()).AsList()
        };
    }
}
