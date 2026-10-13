using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface IScheduleService
{
    Task<PagedResult<ScheduleDto>> SearchAsync(ScheduleQuery query, CancellationToken ct = default);
    Task<ScheduleDto> GetAsync(int id, CancellationToken ct = default);
    Task<ScheduleDto> CreateAsync(CreateScheduleRequest request, int? actorUserId, CancellationToken ct = default);
    Task<ScheduleDto> UpdateAsync(int id, UpdateScheduleRequest request, int? actorUserId, CancellationToken ct = default);
    Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default);
}
