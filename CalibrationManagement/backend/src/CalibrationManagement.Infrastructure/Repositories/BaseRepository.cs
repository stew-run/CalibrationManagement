using Dapper;
using System.Data;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Infrastructure.Data;

namespace CalibrationManagement.Infrastructure.Repositories;

/// <summary>Thin helper around Dapper. Every call is a parameterised stored-procedure call.</summary>
public abstract class BaseRepository
{
    protected IDbConnectionFactory Factory { get; }

    protected BaseRepository(IDbConnectionFactory factory) => Factory = factory;

    private static CommandDefinition Cmd(string procedure, object? param, CancellationToken ct)
        => new(procedure, param, commandType: CommandType.StoredProcedure, commandTimeout: 30, cancellationToken: ct);

    protected async Task<IReadOnlyList<T>> QueryAsync<T>(string procedure, object? param, CancellationToken ct)
    {
        await using var connection = Factory.CreateConnection();
        var rows = await connection.QueryAsync<T>(Cmd(procedure, param, ct));
        return rows.AsList();
    }

    protected async Task<T?> QueryFirstOrDefaultAsync<T>(string procedure, object? param, CancellationToken ct)
    {
        await using var connection = Factory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(Cmd(procedure, param, ct));
    }

    protected async Task<T> QuerySingleAsync<T>(string procedure, object? param, CancellationToken ct)
    {
        await using var connection = Factory.CreateConnection();
        return await connection.QuerySingleAsync<T>(Cmd(procedure, param, ct));
    }

    protected async Task ExecuteAsync(string procedure, object? param, CancellationToken ct)
    {
        await using var connection = Factory.CreateConnection();
        await connection.ExecuteAsync(Cmd(procedure, param, ct));
    }

    protected async Task<PagedResult<T>> QueryPagedAsync<T>(string procedure, object param, int page, int pageSize, CancellationToken ct)
        where T : PagedRow
    {
        var rows = await QueryAsync<T>(procedure, param, ct);
        return new PagedResult<T>
        {
            Items = rows,
            Page = page,
            PageSize = pageSize,
            TotalCount = rows.Count == 0 ? 0 : rows[0].TotalCount
        };
    }

    protected static CommandDefinition MultiCmd(string procedure, object? param, CancellationToken ct) => Cmd(procedure, param, ct);
}
