using CalibrationManagement.Application.Common;

namespace CalibrationManagement.Application.DTOs;

public class CalibrationReportRowDto : PagedRow
{
    public int EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string? RangeLeastCount { get; set; }
    public string? Make { get; set; }
    public string? DepartmentName { get; set; }
    public string? AgencyName { get; set; }
    public string? FrequencyLabel { get; set; }
    public string? ResponsiblePerson { get; set; }
    public DateTime? LastCalibrationDate { get; set; }
    public DateTime? NextDueDate { get; set; }
    public string CalibrationStatus { get; set; } = string.Empty;
    public int? DaysToDue { get; set; }
}

public class CalibrationReportQuery : PagedQuery
{
    public string ReportType { get; set; } = "all";
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? DepartmentId { get; set; }
    public int? AgencyId { get; set; }
    public string? Search { get; set; }
}

public class AgencySummaryDto
{
    public string AgencyName { get; set; } = string.Empty;
    public int TotalCalibrations { get; set; }
    public int OkCount { get; set; }
    public int NotOkCount { get; set; }
    public int UnverifiedCount { get; set; }
    public DateTime? FirstCalibration { get; set; }
    public DateTime? LastCalibration { get; set; }
}

public class DataQualityIssueDto
{
    public int IssueId { get; set; }
    public string SourceFile { get; set; } = string.Empty;
    public int? SourceRow { get; set; }
    public string IssueType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? EquipmentCode { get; set; }
    public int? EquipmentId { get; set; }
    public string Details { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
}

public class ResolveIssueRequest
{
    public bool IsResolved { get; set; } = true;
}
