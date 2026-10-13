using CalibrationManagement.Domain.Entities;

namespace CalibrationManagement.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(User user);
}
