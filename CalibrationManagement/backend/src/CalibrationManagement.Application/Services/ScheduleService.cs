using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Application.Validators;

namespace CalibrationManagement.Application.Services;

public sealed class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _repo;

    public ScheduleService(IScheduleRepository repo) => _repo = repo;

    public Task<PagedResult<ScheduleDto>> SearchAsync(ScheduleQuery query, CancellationToken ct = default)
    {
        if (query.From.HasValue && query.To.HasValue && query.From > query.To)
            throw new AppValidationException("from", "'From' date must not be after the 'To' date.");
        return _repo.SearchAsync(query, ct);
    }

    public async Task<ScheduleDto> GetAsync(int id, CancellationToken ct = default)
        => await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Schedule {id} was not found.");

    public async Task<ScheduleDto> CreateAsync(CreateScheduleRequest request, int? actorUserId, CancellationToken ct = default)
    {
        if (request.EquipmentId <= 0) throw new AppValidationException("equipmentId", "Select an equipment item.");
        request.AssignedTo = Guard.Optional("assignedTo", request.AssignedTo, 100);
        request.Notes = Guard.Optional("notes", request.Notes, 500);
        var id = await _repo.CreateAsync(request, actorUserId, ct);
        return await GetAsync(id, ct);
    }

    public async Task<ScheduleDto> UpdateAsync(int id, UpdateScheduleRequest request, int? actorUserId, CancellationToken ct = default)
    {
        Guard.OneOf("status", request.Status, Guard.ScheduleStatuses);
        request.AssignedTo = Guard.Optional("assignedTo", request.AssignedTo, 100);
        request.Notes = Guard.Optional("notes", request.Notes, 500);
        await _repo.UpdateAsync(id, request, actorUserId, ct);
        return await GetAsync(id, ct);
    }

    public Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default) => _repo.DeleteAsync(id, actorUserId, ct);
}
