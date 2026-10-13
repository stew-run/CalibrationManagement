namespace CalibrationManagement.Application.DTOs;

public class DashboardMetricsDto
{
    public int TotalEquipment { get; set; }
    public int ValidCount { get; set; }
    public int DueSoonCount { get; set; }
    public int OverdueCount { get; set; }
    public int FailedCount { get; set; }
    public int NotCalibratedCount { get; set; }
    public int PlannedThisMonth { get; set; }
    public int OpenMaintenance { get; set; }
    public int ExpiringCertificates { get; set; }
    public int UnreadNotifications { get; set; }
}

public class DepartmentStatDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Overdue { get; set; }
    public int DueSoon { get; set; }
}

public class MonthDueDto
{
    public string MonthKey { get; set; } = string.Empty;
    public int DueCount { get; set; }
}

public class RecentResultDto
{
    public int ResultId { get; set; }
    public int EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public DateTime CalibrationDate { get; set; }
    public string Result { get; set; } = string.Empty;
}

public class UpcomingItemDto
{
    public int EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public DateTime? NextDueDate { get; set; }
    public string CalibrationStatus { get; set; } = string.Empty;
}

public class DashboardDto
{
    public DashboardMetricsDto Metrics { get; set; } = new();
    public IReadOnlyList<DepartmentStatDto> ByDepartment { get; set; } = Array.Empty<DepartmentStatDto>();
    public IReadOnlyList<MonthDueDto> DueByMonth { get; set; } = Array.Empty<MonthDueDto>();
    public IReadOnlyList<RecentResultDto> RecentResults { get; set; } = Array.Empty<RecentResultDto>();
    public IReadOnlyList<UpcomingItemDto> Upcoming { get; set; } = Array.Empty<UpcomingItemDto>();
}
