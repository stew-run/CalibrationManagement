using Dapper;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Infrastructure.Data;

namespace CalibrationManagement.Infrastructure.Repositories;

public sealed class ResultRepository : BaseRepository, IResultRepository
{
    public ResultRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<PagedResult<ResultDto>> SearchAsync(ResultQuery q, CancellationToken ct = default)
        => QueryPagedAsync<ResultDto>("dbo.usp_Results_Search", new
        {
            q.EquipmentId, q.Result, q.AgencyId, q.From, q.To, q.Search, Page = q.SafePage, PageSize = q.SafePageSize
        }, q.SafePage, q.SafePageSize, ct);

    public Task<ResultDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => QueryFirstOrDefaultAsync<ResultDto>("dbo.usp_Results_GetById", new { ResultId = id }, ct);

    public Task<int> CreateAsync(ResultRequest r, DateTime dueDate, int? actorUserId, CancellationToken ct = default)
        => QuerySingleAsync<int>("dbo.usp_Results_Create", new
        {
            r.EquipmentId, r.ScheduleId, CalibrationDate = r.CalibrationDate!.Value, r.CertificateReference, r.AgencyId,
            r.ErrorDetails, r.Result, r.ActionTaken, DueDate = dueDate, r.Remarks, r.SignedBy, ActorUserId = actorUserId
        }, ct);

    public Task UpdateAsync(int id, ResultRequest r, DateTime dueDate, int? actorUserId, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Results_Update", new
        {
            ResultId = id, CalibrationDate = r.CalibrationDate!.Value, r.CertificateReference, r.AgencyId,
            r.ErrorDetails, r.Result, r.ActionTaken, DueDate = dueDate, r.Remarks, r.SignedBy, ActorUserId = actorUserId
        }, ct);

    public Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Results_Delete", new { ResultId = id, ActorUserId = actorUserId }, ct);

    public async Task<HistoryCardDto?> GetHistoryAsync(int equipmentId, CancellationToken ct = default)
    {
        await using var connection = Factory.CreateConnection();
        using var grid = await connection.QueryMultipleAsync(MultiCmd("dbo.usp_History_GetByEquipment", new { EquipmentId = equipmentId }, ct));
        var equipment = await grid.ReadFirstOrDefaultAsync<EquipmentDto>();
        if (equipment is null) return null;
        var rows = (await grid.ReadAsync<HistoryRowDto>()).AsList();
        return new HistoryCardDto { Equipment = equipment, Rows = rows };
    }
}
