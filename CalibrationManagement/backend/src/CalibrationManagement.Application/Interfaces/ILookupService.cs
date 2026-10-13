using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface ILookupService
{
    Task<LookupsDto> GetAllAsync(CancellationToken ct = default);
    Task<LookupItem> CreateDepartmentAsync(CreateLookupRequest request, int? actorUserId, CancellationToken ct = default);
    Task<LookupItem> CreateAgencyAsync(CreateLookupRequest request, int? actorUserId, CancellationToken ct = default);
    Task<bool> CanConnectAsync(CancellationToken ct = default);
}
