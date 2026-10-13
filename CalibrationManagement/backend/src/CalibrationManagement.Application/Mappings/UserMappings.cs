using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Domain.Entities;

namespace CalibrationManagement.Application.Mappings;

public static class UserMappings
{
    /// <summary>Projects the user entity to the client-safe DTO (never exposes the password hash).</summary>
    public static UserDto ToDto(this User user) => new()
    {
        UserId = user.UserId, Username = user.Username, Email = user.Email, FullName = user.FullName, RoleId = user.RoleId,
        Role = user.Role, IsActive = user.IsActive, LastLoginAt = user.LastLoginAt, CreatedAt = user.CreatedAt
    };
}
