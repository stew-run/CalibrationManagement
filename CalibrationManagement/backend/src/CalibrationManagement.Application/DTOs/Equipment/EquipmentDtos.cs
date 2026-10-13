using System.ComponentModel.DataAnnotations;
using CalibrationManagement.Application.Common;

namespace CalibrationManagement.Application.DTOs;

public class EquipmentDto : PagedRow
{
    public int EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public int? SourceSerialNo { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string? RangeLeastCount { get; set; }
    public string? Make { get; set; }
    public string? SerialNo { get; set; }
    public string? AcceptableErrorLimit { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? Location { get; set; }
    public string CalibrationType { get; set; } = "External";
    public int? AgencyId { get; set; }
    public string? AgencyName { get; set; }
    public int? FrequencyId { get; set; }
    public string? FrequencyLabel { get; set; }
    public int? FrequencyMonths { get; set; }
    public string? ResponsiblePerson { get; set; }
    public string? GroupName { get; set; }
    public string Status { get; set; } = "Active";
    public string? Remarks { get; set; }
    public DateTime? LastCalibrationDate { get; set; }
    public DateTime? NextDueDate { get; set; }
    public string CalibrationStatus { get; set; } = "NotCalibrated";
    public int CodeCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class EquipmentRequest
{
    [Required, StringLength(50)] public string EquipmentCode { get; set; } = string.Empty;
    [Required, StringLength(250)] public string EquipmentName { get; set; } = string.Empty;
    [StringLength(150)] public string? RangeLeastCount { get; set; }
    [StringLength(100)] public string? Make { get; set; }
    [StringLength(100)] public string? SerialNo { get; set; }
    [StringLength(80)] public string? AcceptableErrorLimit { get; set; }
    public int? DepartmentId { get; set; }
    [StringLength(150)] public string? Location { get; set; }
    public string CalibrationType { get; set; } = "External";
    public int? AgencyId { get; set; }
    public int? FrequencyId { get; set; }
    [StringLength(100)] public string? ResponsiblePerson { get; set; }
    [StringLength(150)] public string? GroupName { get; set; }
    public string Status { get; set; } = "Active";
    [StringLength(500)] public string? Remarks { get; set; }
    public bool AllowDuplicateCode { get; set; }
}

public class EquipmentQuery : PagedQuery
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public int? AgencyId { get; set; }
    public string? Status { get; set; }
    public string? CalibrationStatus { get; set; }
    public bool DuplicatesOnly { get; set; }
    public string SortBy { get; set; } = "code";
    public string SortDir { get; set; } = "asc";
}
