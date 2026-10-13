using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserDto>> ListAsync(UserQuery query, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, int? actorUserId, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(int userId, UpdateUserRequest request, int? actorUserId, CancellationToken ct = default);
    Task ResetPasswordAsync(int userId, ResetPasswordRequest request, int? actorUserId, CancellationToken ct = default);
    Task<IReadOnlyList<RoleDto>> ListRolesAsync(CancellationToken ct = default);
}
