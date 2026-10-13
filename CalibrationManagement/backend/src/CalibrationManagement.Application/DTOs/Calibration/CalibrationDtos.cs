using System.ComponentModel.DataAnnotations;
using CalibrationManagement.Application.Common;

namespace CalibrationManagement.Application.DTOs;

public class ScheduleDto : PagedRow
{
    public int ScheduleId { get; set; }
    public int EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? AgencyName { get; set; }
    public int PlanYear { get; set; }
    public DateTime DueDate { get; set; }
    public string? AssignedTo { get; set; }
    public string Status { get; set; } = "Planned";
    public string DisplayStatus { get; set; } = "Planned";
    public int DaysUntilDue { get; set; }
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateScheduleRequest
{
    [Range(1, int.MaxValue)] public int EquipmentId { get; set; }
    [Range(2000, 2100)] public int PlanYear { get; set; }
    [Required] public DateTime? DueDate { get; set; }
    [StringLength(100)] public string? AssignedTo { get; set; }
    [StringLength(500)] public string? Notes { get; set; }
}

public class UpdateScheduleRequest
{
    [Range(2000, 2100)] public int PlanYear { get; set; }
    [Required] public DateTime? DueDate { get; set; }
    [StringLength(100)] public string? AssignedTo { get; set; }
    public string Status { get; set; } = "Planned";
    [StringLength(500)] public string? Notes { get; set; }
}

public class ScheduleQuery : PagedQuery
{
    public int? PlanYear { get; set; }
    public string? Status { get; set; }
    public int? EquipmentId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Search { get; set; }
}

public class ResultDto : PagedRow
{
    public int ResultId { get; set; }
    public int EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public int? ScheduleId { get; set; }
    public DateTime CalibrationDate { get; set; }
    public string? CertificateReference { get; set; }
    public int? AgencyId { get; set; }
    public string? AgencyName { get; set; }
    public string? ErrorDetails { get; set; }
    public string Result { get; set; } = "OK";
    public string? ActionTaken { get; set; }
    public DateTime DueDate { get; set; }
    public string? Remarks { get; set; }
    public string? SignedBy { get; set; }
}

public class HistoryRowDto : ResultDto
{
    public int SlNo { get; set; }
}

public class HistoryCardDto
{
    public EquipmentDto Equipment { get; set; } = new();
    public IReadOnlyList<HistoryRowDto> Rows { get; set; } = Array.Empty<HistoryRowDto>();
}

public class ResultRequest
{
    /// <summary>Required on create; ignored on update.</summary>
    public int EquipmentId { get; set; }
    /// <summary>Optional plan entry that this calibration completes (create only).</summary>
    public int? ScheduleId { get; set; }
    [Required] public DateTime? CalibrationDate { get; set; }
    [StringLength(100)] public string? CertificateReference { get; set; }
    public int? AgencyId { get; set; }
    [StringLength(500)] public string? ErrorDetails { get; set; }
    [Required] public string Result { get; set; } = "OK";
    [StringLength(500)] public string? ActionTaken { get; set; }
    /// <summary>Optional: when omitted it is derived from the equipment calibration frequency.</summary>
    public DateTime? DueDate { get; set; }
    [StringLength(500)] public string? Remarks { get; set; }
    [StringLength(100)] public string? SignedBy { get; set; }
}

public class ResultQuery : PagedQuery
{
    public int? EquipmentId { get; set; }
    public string? Result { get; set; }
    public int? AgencyId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Search { get; set; }
}

public class CertificateDto : PagedRow
{
    public int CertificateId { get; set; }
    public int EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public int? ResultId { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public int? AgencyId { get; set; }
    public string? AgencyName { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? StorageLocation { get; set; }
    public string? Notes { get; set; }
}

public class CertificateRequest
{
    /// <summary>Required on create; ignored on update.</summary>
    public int EquipmentId { get; set; }
    public int? ResultId { get; set; }
    [Required, StringLength(100)] public string CertificateNumber { get; set; } = string.Empty;
    public int? AgencyId { get; set; }
    [Required] public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    [StringLength(260)] public string? FileName { get; set; }
    [StringLength(100)] public string? ContentType { get; set; }
    public long? FileSizeBytes { get; set; }
    [StringLength(500)] public string? StorageLocation { get; set; }
    [StringLength(500)] public string? Notes { get; set; }
}

public class CertificateQuery : PagedQuery
{
    public int? EquipmentId { get; set; }
    public string? Search { get; set; }
    public int? ExpiringWithinDays { get; set; }
}

public class MaintenanceDto : PagedRow
{
    public int MaintenanceId { get; set; }
    public int EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string MaintenanceType { get; set; } = string.Empty;
    public DateTime MaintenanceDate { get; set; }
    public string? PerformedBy { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal? Cost { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string Status { get; set; } = "Open";
}

public class MaintenanceRequest
{
    /// <summary>Required on create; ignored on update.</summary>
    public int EquipmentId { get; set; }
    [Required] public string MaintenanceType { get; set; } = "Preventive";
    [Required] public DateTime? MaintenanceDate { get; set; }
    [StringLength(100)] public string? PerformedBy { get; set; }
    [Required, StringLength(1000)] public string Description { get; set; } = string.Empty;
    [Range(0, 999999999)] public decimal? Cost { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string Status { get; set; } = "Open";
}

public class MaintenanceQuery : PagedQuery
{
    public int? EquipmentId { get; set; }
    public string? Status { get; set; }
    public string? MaintenanceType { get; set; }
    public string? Search { get; set; }
}
