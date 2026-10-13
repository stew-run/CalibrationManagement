using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Infrastructure.Data;

namespace CalibrationManagement.Infrastructure.Repositories;

public sealed class EquipmentRepository : BaseRepository, IEquipmentRepository
{
    public EquipmentRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<PagedResult<EquipmentDto>> SearchAsync(EquipmentQuery q, CancellationToken ct = default)
        => QueryPagedAsync<EquipmentDto>("dbo.usp_Equipment_Search", new
        {
            q.Search, q.DepartmentId, q.AgencyId, q.Status, q.CalibrationStatus, q.DuplicatesOnly,
            q.SortBy, q.SortDir, Page = q.SafePage, PageSize = q.SafePageSize
        }, q.SafePage, q.SafePageSize, ct);

    public Task<EquipmentDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => QueryFirstOrDefaultAsync<EquipmentDto>("dbo.usp_Equipment_GetById", new { EquipmentId = id }, ct);

    public Task<int> CreateAsync(EquipmentRequest r, int? actorUserId, CancellationToken ct = default)
        => QuerySingleAsync<int>("dbo.usp_Equipment_Create", new
        {
            r.EquipmentCode, r.EquipmentName, r.RangeLeastCount, r.Make, r.SerialNo, r.AcceptableErrorLimit,
            r.DepartmentId, r.Location, r.CalibrationType, r.AgencyId, r.FrequencyId, r.ResponsiblePerson,
            r.GroupName, r.Status, r.Remarks, r.AllowDuplicateCode, ActorUserId = actorUserId
        }, ct);

    public Task UpdateAsync(int id, EquipmentRequest r, int? actorUserId, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Equipment_Update", new
        {
            EquipmentId = id, r.EquipmentCode, r.EquipmentName, r.RangeLeastCount, r.Make, r.SerialNo, r.AcceptableErrorLimit,
            r.DepartmentId, r.Location, r.CalibrationType, r.AgencyId, r.FrequencyId, r.ResponsiblePerson,
            r.GroupName, r.Status, r.Remarks, r.AllowDuplicateCode, ActorUserId = actorUserId
        }, ct);

    public Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Equipment_Delete", new { EquipmentId = id, ActorUserId = actorUserId }, ct);
}
