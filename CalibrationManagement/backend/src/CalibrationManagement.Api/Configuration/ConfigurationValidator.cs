using CalibrationManagement.Infrastructure.Security;

namespace CalibrationManagement.Api.Configuration;

public static class ConfigurationValidator
{
    public const int MinJwtKeyLength = 32;

    /// <summary>Returns human-readable configuration problems (empty list when the configuration is usable).</summary>
    public static IReadOnlyList<string> Validate(string? connectionString, JwtSettings jwt, bool requireJwt)
    {
        var problems = new List<string>();
        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
            problems.Add("ConnectionStrings:Default is not configured. Copy appsettings.Local.json.example to appsettings.Local.json and set your SQL Server connection string.");

        if (requireJwt)
        {
            var key = jwt.Key ?? string.Empty;
            if (key.Length < MinJwtKeyLength)
                problems.Add($"Jwt:Key must be a random secret of at least {MinJwtKeyLength} characters (set it in appsettings.Local.json or with 'dotnet user-secrets').");
            else if (key.StartsWith("PASTE-", StringComparison.OrdinalIgnoreCase) || key.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase))
                problems.Add("Jwt:Key still contains the placeholder text from the example file. Replace it with your own random secret.");
            if (jwt.ExpiryMinutes is < 1 or > 120)
                problems.Add("Jwt:ExpiryMinutes must be between 1 and 120 (tokens are intentionally short-lived).");
        }
        return problems;
    }
}
