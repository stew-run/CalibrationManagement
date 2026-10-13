using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface IMaintenanceService
{
    Task<PagedResult<MaintenanceDto>> SearchAsync(MaintenanceQuery query, CancellationToken ct = default);
    Task<MaintenanceDto> GetAsync(int id, CancellationToken ct = default);
    Task<MaintenanceDto> CreateAsync(MaintenanceRequest request, int? actorUserId, CancellationToken ct = default);
    Task<MaintenanceDto> UpdateAsync(int id, MaintenanceRequest request, int? actorUserId, CancellationToken ct = default);
    Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default);
}
