using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface IMaintenanceRepository
{
    Task<PagedResult<MaintenanceDto>> SearchAsync(MaintenanceQuery query, CancellationToken ct = default);
    Task<MaintenanceDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(MaintenanceRequest request, int? actorUserId, CancellationToken ct = default);
    Task UpdateAsync(int id, MaintenanceRequest request, int? actorUserId, CancellationToken ct = default);
    Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default);
}
