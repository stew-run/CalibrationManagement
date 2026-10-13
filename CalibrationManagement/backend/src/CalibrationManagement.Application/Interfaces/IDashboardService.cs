using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(int userId, CancellationToken ct = default);
}
