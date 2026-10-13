using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Infrastructure.Security;

public sealed class PasswordService : IPasswordService
{
    private const int MinLength = 12;
    private const int MaxLength = 128;
    private readonly int _workFactor;
    private readonly Lazy<string> _dummyHash;

    public PasswordService(int workFactor = 12)
    {
        _workFactor = workFactor;
        _dummyHash = new Lazy<string>(() => BCrypt.Net.BCrypt.EnhancedHashPassword("timing-equaliser-password", _workFactor));
    }

    public string Hash(string password) => BCrypt.Net.BCrypt.EnhancedHashPassword(password, _workFactor);

    public bool Verify(string password, string? hash)
    {
        try
        {
            if (string.IsNullOrEmpty(hash))
            {
                // Unknown user: spend the same time as a real check so response timing does not reveal account existence.
                BCrypt.Net.BCrypt.EnhancedVerify(password, _dummyHash.Value);
                return false;
            }
            return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
        }
        catch (Exception)
        {
            return false; // malformed hash
        }
    }

    public IReadOnlyList<string> Validate(string password, string? username = null)
    {
        var problems = new List<string>();
        if (string.IsNullOrEmpty(password) || password.Length < MinLength) problems.Add($"Password must be at least {MinLength} characters long.");
        if (password is not null && password.Length > MaxLength) problems.Add($"Password must be at most {MaxLength} characters long.");
        if (password is null) return problems;
        if (!password.Any(char.IsUpper)) problems.Add("Password must contain an uppercase letter.");
        if (!password.Any(char.IsLower)) problems.Add("Password must contain a lowercase letter.");
        if (!password.Any(char.IsDigit)) problems.Add("Password must contain a digit.");
        if (password.Contains("password", StringComparison.OrdinalIgnoreCase)) problems.Add("Password must not contain the word 'password'.");
        if (!string.IsNullOrWhiteSpace(username) && password.Contains(username, StringComparison.OrdinalIgnoreCase))
            problems.Add("Password must not contain the username.");
        return problems;
    }
}
