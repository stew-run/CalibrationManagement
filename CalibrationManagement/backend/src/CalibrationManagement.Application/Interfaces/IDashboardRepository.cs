using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardDto> GetSummaryAsync(int userId, CancellationToken ct = default);
}
