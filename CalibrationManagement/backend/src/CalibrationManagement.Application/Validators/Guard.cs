using CalibrationManagement.Application.Common;

namespace CalibrationManagement.Application.Validators;

public static class Guard
{
    public static readonly string[] EquipmentStatuses = { "Active", "Inactive", "OutOfService", "Retired" };
    public static readonly string[] CalibrationTypes = { "Internal", "External", "Customer" };
    public static readonly string[] ScheduleStatuses = { "Planned", "Completed", "Cancelled" };
    public static readonly string[] ResultValues = { "OK", "NOTOK", "Unverified" };
    public static readonly string[] MaintenanceTypes = { "Preventive", "Corrective", "Repair", "Inspection" };
    public static readonly string[] MaintenanceStatuses = { "Open", "InProgress", "Completed" };

    public static void OneOf(string field, string? value, string[] allowed)
    {
        if (value is null || !allowed.Contains(value, StringComparer.Ordinal))
            throw new AppValidationException(field, $"{field} must be one of: {string.Join(", ", allowed)}.");
    }

    public static string Required(string field, string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed)) throw new AppValidationException(field, $"{field} is required.");
        if (trimmed.Length > maxLength) throw new AppValidationException(field, $"{field} must be at most {maxLength} characters.");
        return trimmed;
    }

    public static string? Optional(string field, string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return null;
        if (trimmed.Length > maxLength) throw new AppValidationException(field, $"{field} must be at most {maxLength} characters.");
        return trimmed;
    }
}
