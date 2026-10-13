using Microsoft.Data.SqlClient;
using System.Text;
using CalibrationManagement.Api.Middleware;
using CalibrationManagement.Application.Common;
using CalibrationManagement.Application.Interfaces;

namespace CalibrationManagement.Api.Provisioning;

/// <summary>
/// One-time command: dotnet run -- create-admin
/// Prompts for the credentials, hashes the password with BCrypt and stores only the hash.
/// Refuses to run when an administrator already exists. No default password exists anywhere.
/// </summary>
public static class AdminProvisioning
{
    public static async Task<int> RunAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provisioning = scope.ServiceProvider.GetRequiredService<IAdminProvisioningService>();

        Console.WriteLine("=== Calibration Management: create the initial administrator ===");
        Console.WriteLine("Password rules: 12+ characters with upper-case, lower-case and a digit. The password is hashed; only the hash is stored.");
        Console.WriteLine();
        var username = Prompt("Username: ");
        var fullName = Prompt("Full name: ");
        var email = Prompt("Email: ");
        var password = ReadPassword("Password: ");
        var confirm = ReadPassword("Confirm password: ");

        if (!string.Equals(password, confirm, StringComparison.Ordinal))
        {
            Console.Error.WriteLine("The passwords do not match. Nothing was created.");
            return 1;
        }

        try
        {
            var id = await provisioning.ProvisionAsync(username, email, fullName, password);
            Console.WriteLine($"Done. Administrator '{username}' was created (UserId {id}). You can now start the API and sign in.");
            return 0;
        }
        catch (AppValidationException ex)
        {
            Console.Error.WriteLine("Validation failed:");
            foreach (var (field, messages) in ex.Errors)
                foreach (var message in messages) Console.Error.WriteLine($"  - {field}: {message}");
            return 1;
        }
        catch (AppException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
        catch (SqlException ex)
        {
            Console.Error.WriteLine(ExceptionHandlingMiddleware.Map(ex).Detail);
            return 1;
        }
    }

    private static string Prompt(string label)
    {
        Console.Write(label);
        return (Console.ReadLine() ?? string.Empty).Trim();
    }

    private static string ReadPassword(string label)
    {
        Console.Write(label);
        if (Console.IsInputRedirected) return Console.ReadLine() ?? string.Empty;

        var buffer = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0) buffer.Length--;
            }
            else if (!char.IsControl(key.KeyChar))
            {
                buffer.Append(key.KeyChar);
            }
        }
        Console.WriteLine();
        return buffer.ToString();
    }
}
