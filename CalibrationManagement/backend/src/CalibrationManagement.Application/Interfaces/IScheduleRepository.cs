using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface IScheduleRepository
{
    Task<PagedResult<ScheduleDto>> SearchAsync(ScheduleQuery query, CancellationToken ct = default);
    Task<ScheduleDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(CreateScheduleRequest request, int? actorUserId, CancellationToken ct = default);
    Task UpdateAsync(int id, UpdateScheduleRequest request, int? actorUserId, CancellationToken ct = default);
    Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default);
}
