using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Application.Mappings;
using CalibrationManagement.Application.Validators;
using CalibrationManagement.Domain.Entities;

namespace CalibrationManagement.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _passwords;

    public UserService(IUserRepository users, IPasswordService passwords)
    {
        _users = users;
        _passwords = passwords;
    }

    public Task<PagedResult<UserDto>> ListAsync(UserQuery query, CancellationToken ct = default) => _users.ListAsync(query, ct);

    public async Task<UserDto> CreateAsync(CreateUserRequest request, int? actorUserId, CancellationToken ct = default)
    {
        var username = Guard.Required("username", request.Username, 50);
        UserRules.ValidateUsername(username);
        UserRules.ValidatePassword(_passwords, "password", request.Password, username);
        var id = await _users.CreateAsync(username, Guard.Required("email", request.Email, 256),
            Guard.Required("fullName", request.FullName, 150), _passwords.Hash(request.Password), request.RoleId, actorUserId, ct);
        var created = await _users.GetByIdAsync(id, ct) ?? throw new NotFoundException("User not found after creation.");
        return created.ToDto();
    }

    public async Task<UserDto> UpdateAsync(int userId, UpdateUserRequest request, int? actorUserId, CancellationToken ct = default)
    {
        if (actorUserId == userId && !request.IsActive)
            throw new BusinessRuleException("You cannot deactivate your own account.");
        await _users.UpdateAsync(userId, Guard.Required("email", request.Email, 256), Guard.Required("fullName", request.FullName, 150),
            request.RoleId, request.IsActive, actorUserId, ct);
        var updated = await _users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("User not found.");
        return updated.ToDto();
    }

    public async Task ResetPasswordAsync(int userId, ResetPasswordRequest request, int? actorUserId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("User not found.");
        UserRules.ValidatePassword(_passwords, "newPassword", request.NewPassword, user.Username);
        await _users.ChangePasswordAsync(userId, _passwords.Hash(request.NewPassword), actorUserId, ct);
    }

    public Task<IReadOnlyList<RoleDto>> ListRolesAsync(CancellationToken ct = default) => _users.ListRolesAsync(ct);
}
