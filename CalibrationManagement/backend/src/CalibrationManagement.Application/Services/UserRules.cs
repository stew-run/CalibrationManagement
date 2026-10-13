using System.Text.RegularExpressions;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Application.Services;

internal static class UserRules
{
    private static readonly Regex UsernamePattern = new(@"^[A-Za-z0-9._-]{3,50}$", RegexOptions.Compiled);

    public static void ValidateUsername(string username)
    {
        if (!UsernamePattern.IsMatch(username))
            throw new AppValidationException("username", "Username must be 3-50 characters: letters, digits, '.', '_' or '-'.");
    }

    public static void ValidatePassword(IPasswordService passwords, string field, string password, string? username)
    {
        var problems = passwords.Validate(password, username);
        if (problems.Count > 0)
            throw new AppValidationException(new Dictionary<string, string[]> { [field] = problems.ToArray() });
    }
}
