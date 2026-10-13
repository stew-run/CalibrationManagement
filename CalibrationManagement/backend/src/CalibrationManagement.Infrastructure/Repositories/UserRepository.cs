using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Domain.Entities;
using CalibrationManagement.Infrastructure.Data;

namespace CalibrationManagement.Infrastructure.Repositories;

public sealed class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<User?> GetByUsernameAsync(string usernameOrEmail, CancellationToken ct = default)
        => QueryFirstOrDefaultAsync<User>("dbo.usp_Users_GetByUsername", new { Username = usernameOrEmail }, ct);

    public Task<User?> GetByIdAsync(int userId, CancellationToken ct = default)
        => QueryFirstOrDefaultAsync<User>("dbo.usp_Users_GetById", new { UserId = userId }, ct);

    public Task<PagedResult<UserDto>> ListAsync(UserQuery query, CancellationToken ct = default)
        => QueryPagedAsync<UserDto>("dbo.usp_Users_List",
            new { query.Search, Page = query.SafePage, PageSize = query.SafePageSize }, query.SafePage, query.SafePageSize, ct);

    public Task<int> CreateAsync(string username, string email, string fullName, string passwordHash, int roleId, int? actorUserId, CancellationToken ct = default)
        => QuerySingleAsync<int>("dbo.usp_Users_Create",
            new { Username = username, Email = email, FullName = fullName, PasswordHash = passwordHash, RoleId = (byte)roleId, ActorUserId = actorUserId }, ct);

    public Task UpdateAsync(int userId, string email, string fullName, int roleId, bool isActive, int? actorUserId, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Users_Update",
            new { UserId = userId, Email = email, FullName = fullName, RoleId = (byte)roleId, IsActive = isActive, ActorUserId = actorUserId }, ct);

    public Task UpdateProfileAsync(int userId, string email, string fullName, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Users_UpdateProfile", new { UserId = userId, Email = email, FullName = fullName }, ct);

    public Task ChangePasswordAsync(int userId, string passwordHash, int? actorUserId, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Users_ChangePassword", new { UserId = userId, PasswordHash = passwordHash, ActorUserId = actorUserId }, ct);

    public Task RecordLoginAsync(int userId, bool success, int maxFailures, int lockoutMinutes, CancellationToken ct = default)
        => ExecuteAsync("dbo.usp_Users_RecordLogin",
            new { UserId = userId, Success = success, MaxFailures = maxFailures, LockoutMinutes = lockoutMinutes }, ct);

    public Task<int> CreateInitialAdminAsync(string username, string email, string fullName, string passwordHash, CancellationToken ct = default)
        => QuerySingleAsync<int>("dbo.usp_Users_CreateInitialAdmin",
            new { Username = username, Email = email, FullName = fullName, PasswordHash = passwordHash }, ct);

    public async Task<IReadOnlyList<RoleDto>> ListRolesAsync(CancellationToken ct = default)
        => await QueryAsync<RoleDto>("dbo.usp_Roles_List", null, ct);
}
