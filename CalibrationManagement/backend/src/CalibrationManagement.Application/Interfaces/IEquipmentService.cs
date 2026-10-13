using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface IEquipmentService
{
    Task<PagedResult<EquipmentDto>> SearchAsync(EquipmentQuery query, CancellationToken ct = default);
    Task<EquipmentDto> GetAsync(int id, CancellationToken ct = default);
    Task<EquipmentDto> CreateAsync(EquipmentRequest request, int? actorUserId, CancellationToken ct = default);
    Task<EquipmentDto> UpdateAsync(int id, EquipmentRequest request, int? actorUserId, CancellationToken ct = default);
    Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default);
}
