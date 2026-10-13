using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Application.Validators;

namespace CalibrationManagement.Application.Services;

public sealed class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceRepository _repo;

    public MaintenanceService(IMaintenanceRepository repo) => _repo = repo;

    public Task<PagedResult<MaintenanceDto>> SearchAsync(MaintenanceQuery query, CancellationToken ct = default) => _repo.SearchAsync(query, ct);

    public async Task<MaintenanceDto> GetAsync(int id, CancellationToken ct = default)
        => await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Maintenance record {id} was not found.");

    public async Task<MaintenanceDto> CreateAsync(MaintenanceRequest request, int? actorUserId, CancellationToken ct = default)
    {
        if (request.EquipmentId <= 0) throw new AppValidationException("equipmentId", "Select an equipment item.");
        Validate(request);
        var id = await _repo.CreateAsync(request, actorUserId, ct);
        return await GetAsync(id, ct);
    }

    public async Task<MaintenanceDto> UpdateAsync(int id, MaintenanceRequest request, int? actorUserId, CancellationToken ct = default)
    {
        Validate(request);
        await _repo.UpdateAsync(id, request, actorUserId, ct);
        return await GetAsync(id, ct);
    }

    public Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default) => _repo.DeleteAsync(id, actorUserId, ct);

    internal static void Validate(MaintenanceRequest r)
    {
        Guard.OneOf("maintenanceType", r.MaintenanceType, Guard.MaintenanceTypes);
        Guard.OneOf("status", r.Status, Guard.MaintenanceStatuses);
        if (!r.MaintenanceDate.HasValue) throw new AppValidationException("maintenanceDate", "Maintenance date is required.");
        r.Description = Guard.Required("description", r.Description, 1000);
        r.PerformedBy = Guard.Optional("performedBy", r.PerformedBy, 100);
        if (r.NextMaintenanceDate.HasValue && r.NextMaintenanceDate.Value < r.MaintenanceDate.Value)
            throw new AppValidationException("nextMaintenanceDate", "Next maintenance date cannot be earlier than the maintenance date.");
    }
}
