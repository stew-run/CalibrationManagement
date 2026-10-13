using System.Globalization;
using System.Text;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.DTOs;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Application.Services;

public sealed class ReportService : IReportService
{
    private static readonly string[] ReportTypes = { "all", "due", "overdue", "failed", "notcalibrated" };
    private readonly IReportRepository _repo;

    public ReportService(IReportRepository repo) => _repo = repo;

    public Task<PagedResult<CalibrationReportRowDto>> CalibrationAsync(CalibrationReportQuery query, CancellationToken ct = default)
    {
        Validate(query);
        return _repo.CalibrationAsync(query, false, ct);
    }

    public async Task<byte[]> ExportCalibrationCsvAsync(CalibrationReportQuery query, CancellationToken ct = default)
    {
        Validate(query);
        var result = await _repo.CalibrationAsync(query, true, ct);
        return BuildCsv(result.Items);
    }

    public Task<IReadOnlyList<AgencySummaryDto>> AgencySummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        if (from.HasValue && to.HasValue && from > to) throw new AppValidationException("from", "'From' date must not be after the 'To' date.");
        return _repo.AgencySummaryAsync(from, to, ct);
    }

    public Task<IReadOnlyList<DataQualityIssueDto>> DataQualityAsync(bool onlyOpen, CancellationToken ct = default) => _repo.DataQualityAsync(onlyOpen, ct);

    public Task ResolveDataQualityAsync(int issueId, bool isResolved, int? actorUserId, CancellationToken ct = default)
        => _repo.ResolveDataQualityAsync(issueId, isResolved, actorUserId, ct);

    private static void Validate(CalibrationReportQuery q)
    {
        q.ReportType = (q.ReportType ?? "all").ToLowerInvariant();
        if (!ReportTypes.Contains(q.ReportType)) throw new AppValidationException("reportType", $"reportType must be one of: {string.Join(", ", ReportTypes)}.");
        if (q.From.HasValue && q.To.HasValue && q.From > q.To) throw new AppValidationException("from", "'From' date must not be after the 'To' date.");
    }

    /// <summary>Builds a UTF-8 (with BOM, so Excel opens it correctly) CSV. Text cells are neutralised against spreadsheet formula injection.</summary>
    public static byte[] BuildCsv(IEnumerable<CalibrationReportRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Equipment Code,Equipment Name,Range & Least Count,Make,Department,Calibration Agency,Frequency,Responsible Person,Last Calibration,Next Due Date,Status,Days To Due");
        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(",", new[]
            {
                CsvText(r.EquipmentCode), CsvText(r.EquipmentName), CsvText(r.RangeLeastCount), CsvText(r.Make), CsvText(r.DepartmentName),
                CsvText(r.AgencyName), CsvText(r.FrequencyLabel), CsvText(r.ResponsiblePerson), CsvDate(r.LastCalibrationDate),
                CsvDate(r.NextDueDate), CsvText(r.CalibrationStatus), r.DaysToDue?.ToString(CultureInfo.InvariantCulture) ?? string.Empty
            }));
        }
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        return encoding.GetPreamble().Concat(encoding.GetBytes(sb.ToString())).ToArray();
    }

    public static string CsvText(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var v = value;
        if (v[0] is '=' or '+' or '-' or '@' or '\t' or '\r') v = "'" + v;
        if (v.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0) v = "\"" + v.Replace("\"", "\"\"") + "\"";
        return v;
    }

    private static string CsvDate(DateTime? d) => d.HasValue ? d.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty;
}
