using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;

namespace CalibrationManagement.Application.Interfaces;

public interface IReportService
{
    Task<PagedResult<CalibrationReportRowDto>> CalibrationAsync(CalibrationReportQuery query, CancellationToken ct = default);
    Task<byte[]> ExportCalibrationCsvAsync(CalibrationReportQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<AgencySummaryDto>> AgencySummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<IReadOnlyList<DataQualityIssueDto>> DataQualityAsync(bool onlyOpen, CancellationToken ct = default);
    Task ResolveDataQualityAsync(int issueId, bool isResolved, int? actorUserId, CancellationToken ct = default);
}
