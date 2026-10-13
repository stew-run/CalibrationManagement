using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Application.Validators;

namespace CalibrationManagement.Application.Services;

public sealed class AdminProvisioningService : IAdminProvisioningService
{
    private readonly IUserRepository _users;
    private readonly IPasswordService _passwords;

    public AdminProvisioningService(IUserRepository users, IPasswordService passwords)
    {
        _users = users;
        _passwords = passwords;
    }

    public async Task<int> ProvisionAsync(string username, string email, string fullName, string password, CancellationToken ct = default)
    {
        username = Guard.Required("username", username, 50);
        UserRules.ValidateUsername(username);
        email = Guard.Required("email", email, 256);
        if (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
            throw new AppValidationException("email", "Enter a valid email address.");
        fullName = Guard.Required("fullName", fullName, 150);
        UserRules.ValidatePassword(_passwords, "password", password, username);
        return await _users.CreateInitialAdminAsync(username, email, fullName, _passwords.Hash(password), ct);
    }
}
