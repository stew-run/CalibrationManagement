namespace CalibrationManagement.Infrastructure.Security;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "CalibrationManagement.Api";
    public string Audience { get; set; } = "CalibrationManagement.Web";
    public string Key { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 30;
}
