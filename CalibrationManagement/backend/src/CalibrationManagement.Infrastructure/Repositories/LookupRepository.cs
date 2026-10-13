using Dapper;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Infrastructure.Data;

namespace CalibrationManagement.Infrastructure.Repositories;

public sealed class LookupRepository : BaseRepository, ILookupRepository
{
    public LookupRepository(IDbConnectionFactory factory) : base(factory) { }

    public async Task<LookupsDto> GetAllAsync(CancellationToken ct = default)
    {
        await using var connection = Factory.CreateConnection();
        using var grid = await connection.QueryMultipleAsync(MultiCmd("dbo.usp_Lookups_Get", null, ct));
        return new LookupsDto
        {
            Departments = (await grid.ReadAsync<LookupItem>()).AsList(),
            Agencies = (await grid.ReadAsync<LookupItem>()).AsList(),
            Frequencies = (await grid.ReadAsync<FrequencyItem>()).AsList(),
            Roles = (await grid.ReadAsync<LookupItem>()).AsList()
        };
    }

    public Task<LookupItem> CreateDepartmentAsync(string name, int? actorUserId, CancellationToken ct = default)
        => QuerySingleAsync<LookupItem>("dbo.usp_Departments_Create", new { Name = name, ActorUserId = actorUserId }, ct);

    public Task<LookupItem> CreateAgencyAsync(string name, int? actorUserId, CancellationToken ct = default)
        => QuerySingleAsync<LookupItem>("dbo.usp_Agencies_Create", new { Name = name, ActorUserId = actorUserId }, ct);

    public async Task<bool> CanConnectAsync(CancellationToken ct = default)
    {
        try
        {
            await using var connection = Factory.CreateConnection();
            await connection.OpenAsync(ct);
            return await connection.ExecuteScalarAsync<int>(new CommandDefinition("SELECT 1", cancellationToken: ct)) == 1;
        }
        catch (Exception) when (!ct.IsCancellationRequested)
        {
            return false;
        }
    }
}
