namespace CalibrationManagement.Application.Interfaces;

public interface IAdminProvisioningService
{
    Task<int> ProvisionAsync(string username, string email, string fullName, string password, CancellationToken ct = default);
}
