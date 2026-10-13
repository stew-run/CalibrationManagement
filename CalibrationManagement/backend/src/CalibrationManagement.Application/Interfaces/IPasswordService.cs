namespace CalibrationManagement.Application.Interfaces;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string? hash);
    /// <summary>Returns a list of problems; empty when the password meets the policy.</summary>
    IReadOnlyList<string> Validate(string password, string? username = null);
}
