using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface IResultRepository
{
    Task<PagedResult<ResultDto>> SearchAsync(ResultQuery query, CancellationToken ct = default);
    Task<ResultDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(ResultRequest request, DateTime dueDate, int? actorUserId, CancellationToken ct = default);
    Task UpdateAsync(int id, ResultRequest request, DateTime dueDate, int? actorUserId, CancellationToken ct = default);
    Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default);
    Task<HistoryCardDto?> GetHistoryAsync(int equipmentId, CancellationToken ct = default);
}
