using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Infrastructure.Data;

namespace CalibrationManagement.Infrastructure.Repositories;

public sealed class ScheduleRepository : BaseRepository, IScheduleRepository
{
    public ScheduleRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<PagedResult<ScheduleDto>> SearchAsync(ScheduleQuery q, CancellationToken ct = default)
        => QueryPagedAsync<ScheduleDto>("dbo.usp_Schedules_Search", new
        {
            q.PlanYear, q.Status, q.EquipmentId, q.DepartmentId, q.From, q.To, q.Search,
            Page = q.SafePage, PageSize = q.SafePageSize
        }, q.SafePage, q.SafePageSize, ct);

    public Task<ScheduleDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => QueryFirstOrDefaultAsync<ScheduleDto>("dbo.usp_Schedules_GetById", new { ScheduleId = id }, ct);

    public Task<int> CreateAsync(CreateScheduleRequest r, int? actorUserId, CancellationToken ct = default)
        => QuerySingleAsync<int>("dbo.usp_Schedules_Create", new
        {
            r.EquipmentId, r.PlanYear, DueDate = r.DueDate!.Value, r.AssignedTo, r.Notes, ActorUserId = actorUserId
        }, ct);

    public Task UpdateAsync(int id, UpdateScheduleRequest r, int? actorUserId, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Schedules_Update", new
        {
            ScheduleId = id, r.PlanYear, DueDate = r.DueDate!.Value, r.AssignedTo, r.Status, r.Notes, ActorUserId = actorUserId
        }, ct);

    public Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Schedules_Delete", new { ScheduleId = id, ActorUserId = actorUserId }, ct);
}
