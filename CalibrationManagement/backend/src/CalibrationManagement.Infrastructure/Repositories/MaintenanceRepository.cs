using Dapper;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Infrastructure.Data;

namespace CalibrationManagement.Infrastructure.Repositories;

public sealed class MaintenanceRepository : BaseRepository, IMaintenanceRepository
{
    public MaintenanceRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<PagedResult<MaintenanceDto>> SearchAsync(MaintenanceQuery q, CancellationToken ct = default)
        => QueryPagedAsync<MaintenanceDto>("dbo.usp_Maintenance_Search", new
        {
            q.EquipmentId, q.Status, q.MaintenanceType, q.Search, Page = q.SafePage, PageSize = q.SafePageSize
        }, q.SafePage, q.SafePageSize, ct);

    public Task<MaintenanceDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => QueryFirstOrDefaultAsync<MaintenanceDto>("dbo.usp_Maintenance_GetById", new { MaintenanceId = id }, ct);

    public Task<int> CreateAsync(MaintenanceRequest r, int? actorUserId, CancellationToken ct = default)
        => QuerySingleAsync<int>("dbo.usp_Maintenance_Create", new
        {
            r.EquipmentId, r.MaintenanceType, MaintenanceDate = r.MaintenanceDate!.Value, r.PerformedBy, r.Description,
            r.Cost, r.NextMaintenanceDate, r.Status, ActorUserId = actorUserId
        }, ct);

    public Task UpdateAsync(int id, MaintenanceRequest r, int? actorUserId, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Maintenance_Update", new
        {
            MaintenanceId = id, r.MaintenanceType, MaintenanceDate = r.MaintenanceDate!.Value, r.PerformedBy, r.Description,
            r.Cost, r.NextMaintenanceDate, r.Status, ActorUserId = actorUserId
        }, ct);

    public Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Maintenance_Delete", new { MaintenanceId = id, ActorUserId = actorUserId }, ct);
}
