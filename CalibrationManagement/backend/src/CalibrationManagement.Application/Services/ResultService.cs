using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Application.Validators;

namespace CalibrationManagement.Application.Services;

public sealed class ResultService : IResultService
{
    private readonly IResultRepository _repo;
    private readonly IEquipmentRepository _equipment;

    public ResultService(IResultRepository repo, IEquipmentRepository equipment)
    {
        _repo = repo;
        _equipment = equipment;
    }

    public Task<PagedResult<ResultDto>> SearchAsync(ResultQuery query, CancellationToken ct = default) => _repo.SearchAsync(query, ct);

    public async Task<ResultDto> GetAsync(int id, CancellationToken ct = default)
        => await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Calibration result {id} was not found.");

    public async Task<ResultDto> CreateAsync(ResultRequest request, int? actorUserId, CancellationToken ct = default)
    {
        if (request.EquipmentId <= 0) throw new AppValidationException("equipmentId", "Select an equipment item.");
        ValidateCommon(request);
        var due = await ResolveDueDateAsync(request, request.EquipmentId, ct);
        var id = await _repo.CreateAsync(request, due, actorUserId, ct);
        return await GetAsync(id, ct);
    }

    public async Task<ResultDto> UpdateAsync(int id, ResultRequest request, int? actorUserId, CancellationToken ct = default)
    {
        var existing = await GetAsync(id, ct);
        ValidateCommon(request);
        var due = await ResolveDueDateAsync(request, existing.EquipmentId, ct);
        await _repo.UpdateAsync(id, request, due, actorUserId, ct);
        return await GetAsync(id, ct);
    }

    public Task DeleteAsync(int id, int? actorUserId, CancellationToken ct = default) => _repo.DeleteAsync(id, actorUserId, ct);

    public async Task<HistoryCardDto> GetHistoryAsync(int equipmentId, CancellationToken ct = default)
        => await _repo.GetHistoryAsync(equipmentId, ct) ?? throw new NotFoundException($"Equipment {equipmentId} was not found.");

    internal static void ValidateCommon(ResultRequest r)
    {
        Guard.OneOf("result", r.Result, Guard.ResultValues);
        if (!r.CalibrationDate.HasValue) throw new AppValidationException("calibrationDate", "Calibration date is required.");
        if (r.CalibrationDate.Value.Date > DateTime.Today.AddDays(1))
            throw new AppValidationException("calibrationDate", "Calibration date cannot be in the future.");
        r.CertificateReference = Guard.Optional("certificateReference", r.CertificateReference, 100);
        r.ErrorDetails = Guard.Optional("errorDetails", r.ErrorDetails, 500);
        r.ActionTaken = Guard.Optional("actionTaken", r.ActionTaken, 500);
        r.Remarks = Guard.Optional("remarks", r.Remarks, 500);
        r.SignedBy = Guard.Optional("signedBy", r.SignedBy, 100);
        if (r.Result == "NOTOK" && string.IsNullOrEmpty(r.ActionTaken))
            throw new AppValidationException("actionTaken", "Action taken is required when the result is NOTOK.");
    }

    internal async Task<DateTime> ResolveDueDateAsync(ResultRequest r, int equipmentId, CancellationToken ct)
    {
        var calibrationDate = r.CalibrationDate!.Value.Date;
        DateTime due;
        if (r.DueDate.HasValue)
        {
            due = r.DueDate.Value.Date;
        }
        else
        {
            var equipment = await _equipment.GetByIdAsync(equipmentId, ct) ?? throw new NotFoundException($"Equipment {equipmentId} was not found.");
            if (equipment.FrequencyMonths is not int months)
                throw new AppValidationException("dueDate", "Due date is required because this equipment has no calibration frequency.");
            // Same convention as the Excel plan: calibrated 11.11.2025 with a 1-year frequency is due 10.11.2026.
            due = calibrationDate.AddMonths(months).AddDays(-1);
        }
        if (due < calibrationDate) throw new AppValidationException("dueDate", "Due date cannot be earlier than the calibration date.");
        return due;
    }
}
