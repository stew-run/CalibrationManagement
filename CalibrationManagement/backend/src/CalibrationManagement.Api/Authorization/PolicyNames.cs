namespace CalibrationManagement.Api.Authorization;

public static class PolicyNames
{
    /// <summary>Any authenticated user.</summary>
    public const string Read = "Read";
    /// <summary>Administrator, QualityManager, Technician.</summary>
    public const string Write = "Write";
    /// <summary>Administrator, QualityManager (deletes, reports on data quality).</summary>
    public const string Manage = "Manage";
    /// <summary>Administrator only (user and role management).</summary>
    public const string AdminOnly = "AdminOnly";
}
