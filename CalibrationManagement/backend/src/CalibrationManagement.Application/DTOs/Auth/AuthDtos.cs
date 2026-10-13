using System.ComponentModel.DataAnnotations;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Domain.Entities;

namespace CalibrationManagement.Application.DTOs;

public class LoginRequest
{
    [Required, StringLength(256)] public string Username { get; set; } = string.Empty;
    [Required, StringLength(256)] public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public UserDto User { get; set; } = new();
}

public class UpdateProfileRequest
{
    [Required, StringLength(150, MinimumLength = 2)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    [Required, StringLength(128)] public string CurrentPassword { get; set; } = string.Empty;
    [Required, StringLength(128)] public string NewPassword { get; set; } = string.Empty;
}

public class UserDto : PagedRow
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserQuery : PagedQuery
{
    public string? Search { get; set; }
}

public class CreateUserRequest
{
    [Required, StringLength(50, MinimumLength = 3)] public string Username { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(150, MinimumLength = 2)] public string FullName { get; set; } = string.Empty;
    [Required, StringLength(128)] public string Password { get; set; } = string.Empty;
    [Range(1, 255)] public int RoleId { get; set; }
}

public class UpdateUserRequest
{
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(150, MinimumLength = 2)] public string FullName { get; set; } = string.Empty;
    [Range(1, 255)] public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ResetPasswordRequest
{
    [Required, StringLength(128)] public string NewPassword { get; set; } = string.Empty;
}

public class RoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
