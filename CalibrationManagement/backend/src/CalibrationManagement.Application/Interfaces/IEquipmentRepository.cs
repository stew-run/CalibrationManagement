using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface IEquipmentRepository
{
    Task<PagedResult<EquipmentDto>> SearchAsync(EquipmentQuery query, CancellationToken ct = default);
    Task<EquipmentDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(EquipmentRequest request, int? actorUserId, CancellationToken ct = default);
    Task UpdateAsync(int id, EquipmentRequest request, int? actorUserId, CancellationToken ct = default);
    Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default);
}
