using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CalibrationManagement.Application;

public static class DependencyInjection
{
    /// <summary>Registers the business-logic services. Their dependencies (repositories, password/JWT services) come from AddInfrastructure.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAdminProvisioningService, AdminProvisioningService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IResultService, ResultService>();
        services.AddScoped<ICertificateService, CertificateService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        return services;
    }
}
