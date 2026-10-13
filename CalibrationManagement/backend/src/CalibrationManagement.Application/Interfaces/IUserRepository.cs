using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Domain.Entities;

namespace CalibrationManagement.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string usernameOrEmail, CancellationToken ct = default);
    Task<User?> GetByIdAsync(int userId, CancellationToken ct = default);
    Task<PagedResult<UserDto>> ListAsync(UserQuery query, CancellationToken ct = default);
    Task<int> CreateAsync(string username, string email, string fullName, string passwordHash, int roleId, int? actorUserId, CancellationToken ct = default);
    Task UpdateAsync(int userId, string email, string fullName, int roleId, bool isActive, int? actorUserId, CancellationToken ct = default);
    Task UpdateProfileAsync(int userId, string email, string fullName, CancellationToken ct = default);
    Task ChangePasswordAsync(int userId, string passwordHash, int? actorUserId, CancellationToken ct = default);
    Task RecordLoginAsync(int userId, bool success, int maxFailures, int lockoutMinutes, CancellationToken ct = default);
    Task<int> CreateInitialAdminAsync(string username, string email, string fullName, string passwordHash, CancellationToken ct = default);
    Task<IReadOnlyList<RoleDto>> ListRolesAsync(CancellationToken ct = default);
}
