using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface ILookupRepository
{
    Task<LookupsDto> GetAllAsync(CancellationToken ct = default);
    Task<LookupItem> CreateDepartmentAsync(string name, int? actorUserId, CancellationToken ct = default);
    Task<LookupItem> CreateAgencyAsync(string name, int? actorUserId, CancellationToken ct = default);
    Task<bool> CanConnectAsync(CancellationToken ct = default);
}
