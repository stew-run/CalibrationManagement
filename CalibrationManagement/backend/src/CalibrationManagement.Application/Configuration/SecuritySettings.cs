namespace CalibrationManagement.Application.Configuration;

public sealed class SecuritySettings
{
    public const string SectionName = "Security";
    public int MaxFailedLogins { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
}
