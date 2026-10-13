using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Application.Validators;

namespace CalibrationManagement.Application.Services;

public sealed class EquipmentService : IEquipmentService
{
    private readonly IEquipmentRepository _repo;

    public EquipmentService(IEquipmentRepository repo) => _repo = repo;

    public Task<PagedResult<EquipmentDto>> SearchAsync(EquipmentQuery query, CancellationToken ct = default)
    {
        var allowedSort = new[] { "code", "name", "department", "due" };
        if (!allowedSort.Contains(query.SortBy)) query.SortBy = "code";
        query.SortDir = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
        return _repo.SearchAsync(query, ct);
    }

    public async Task<EquipmentDto> GetAsync(int id, CancellationToken ct = default)
        => await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Equipment {id} was not found.");

    public async Task<EquipmentDto> CreateAsync(EquipmentRequest request, int? actorUserId, CancellationToken ct = default)
    {
        var clean = Normalize(request);
        var id = await _repo.CreateAsync(clean, actorUserId, ct);
        return await GetAsync(id, ct);
    }

    public async Task<EquipmentDto> UpdateAsync(int id, EquipmentRequest request, int? actorUserId, CancellationToken ct = default)
    {
        var clean = Normalize(request);
        await _repo.UpdateAsync(id, clean, actorUserId, ct);
        return await GetAsync(id, ct);
    }

    public Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default) => _repo.DeleteAsync(id, actorUserId, ct);

    internal static EquipmentRequest Normalize(EquipmentRequest r)
    {
        Guard.OneOf("status", r.Status, Guard.EquipmentStatuses);
        Guard.OneOf("calibrationType", r.CalibrationType, Guard.CalibrationTypes);
        return new EquipmentRequest
        {
            EquipmentCode = Guard.Required("equipmentCode", r.EquipmentCode, 50),
            EquipmentName = Guard.Required("equipmentName", r.EquipmentName, 250),
            RangeLeastCount = Guard.Optional("rangeLeastCount", r.RangeLeastCount, 150),
            Make = Guard.Optional("make", r.Make, 100),
            SerialNo = Guard.Optional("serialNo", r.SerialNo, 100),
            AcceptableErrorLimit = Guard.Optional("acceptableErrorLimit", r.AcceptableErrorLimit, 80),
            DepartmentId = r.DepartmentId,
            Location = Guard.Optional("location", r.Location, 150),
            CalibrationType = r.CalibrationType,
            AgencyId = r.AgencyId,
            FrequencyId = r.FrequencyId,
            ResponsiblePerson = Guard.Optional("responsiblePerson", r.ResponsiblePerson, 100),
            GroupName = Guard.Optional("groupName", r.GroupName, 150),
            Status = r.Status,
            Remarks = Guard.Optional("remarks", r.Remarks, 500),
            AllowDuplicateCode = r.AllowDuplicateCode
        };
    }
}
