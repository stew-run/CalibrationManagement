using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface IResultService
{
    Task<PagedResult<ResultDto>> SearchAsync(ResultQuery query, CancellationToken ct = default);
    Task<ResultDto> GetAsync(int id, CancellationToken ct = default);
    Task<ResultDto> CreateAsync(ResultRequest request, int? actorUserId, CancellationToken ct = default);
    Task<ResultDto> UpdateAsync(int id, ResultRequest request, int? actorUserId, CancellationToken ct = default);
    Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default);
    Task<HistoryCardDto> GetHistoryAsync(int equipmentId, CancellationToken ct = default);
}
