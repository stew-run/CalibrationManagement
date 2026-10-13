using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Application.Validators;

namespace CalibrationManagement.Application.Services;

public sealed class LookupService : ILookupService
{
    private readonly ILookupRepository _repo;

    public LookupService(ILookupRepository repo) => _repo = repo;

    public Task<LookupsDto> GetAllAsync(CancellationToken ct = default) => _repo.GetAllAsync(ct);

    public Task<LookupItem> CreateDepartmentAsync(CreateLookupRequest request, int? actorUserId, CancellationToken ct = default)
        => _repo.CreateDepartmentAsync(Guard.Required("name", request.Name, 150), actorUserId, ct);

    public Task<LookupItem> CreateAgencyAsync(CreateLookupRequest request, int? actorUserId, CancellationToken ct = default)
        => _repo.CreateAgencyAsync(Guard.Required("name", request.Name, 200), actorUserId, ct);

    public Task<bool> CanConnectAsync(CancellationToken ct = default) => _repo.CanConnectAsync(ct);
}
