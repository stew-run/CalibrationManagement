using Microsoft.Extensions.Options;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.Configuration;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Application.Mappings;
using CalibrationManagement.Application.Validators;
using CalibrationManagement.Domain.Entities;

namespace CalibrationManagement.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _passwords;
    private readonly IJwtTokenService _tokens;
    private readonly SecuritySettings _security;

    public AuthService(IUserRepository users, IPasswordService passwords, IJwtTokenService tokens, IOptions<SecuritySettings> security)
    {
        _users = users;
        _passwords = passwords;
        _tokens = tokens;
        _security = security.Value;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        const string invalid = "Invalid username or password.";
        var user = await _users.GetByUsernameAsync(request.Username.Trim(), ct);
        if (user is null)
        {
            _passwords.Verify(request.Password, null);
            throw new UnauthorizedAppException(invalid);
        }

        if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.UtcNow)
            throw new UnauthorizedAppException("This account is temporarily locked after repeated failed sign-ins. Try again later.");

        if (!_passwords.Verify(request.Password, user.PasswordHash))
        {
            await _users.RecordLoginAsync(user.UserId, false, _security.MaxFailedLogins, _security.LockoutMinutes, ct);
            throw new UnauthorizedAppException(invalid);
        }

        if (!user.IsActive)
            throw new UnauthorizedAppException("This account is disabled. Contact an administrator.");

        await _users.RecordLoginAsync(user.UserId, true, _security.MaxFailedLogins, _security.LockoutMinutes, ct);
        var (token, expires) = _tokens.CreateToken(user);
        return new LoginResponse { Token = token, ExpiresAtUtc = expires, User = user.ToDto() };
    }

    public async Task<UserDto> GetProfileAsync(int userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("User not found.");
        return user.ToDto();
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var fullName = Guard.Required("fullName", request.FullName, 150);
        var email = Guard.Required("email", request.Email, 256);
        await _users.UpdateProfileAsync(userId, email, fullName, ct);
        return await GetProfileAsync(userId, ct);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct) ?? throw new NotFoundException("User not found.");
        if (!_passwords.Verify(request.CurrentPassword, user.PasswordHash))
            throw new AppValidationException("currentPassword", "Current password is incorrect.");
        if (request.CurrentPassword == request.NewPassword)
            throw new AppValidationException("newPassword", "The new password must differ from the current password.");
        UserRules.ValidatePassword(_passwords, "newPassword", request.NewPassword, user.Username);
        await _users.ChangePasswordAsync(userId, _passwords.Hash(request.NewPassword), userId, ct);
    }
}
