using CalibrationManagement.Application.Interfaces;
using CalibrationManagement.Infrastructure.Data;
using CalibrationManagement.Infrastructure.Repositories;
using CalibrationManagement.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace CalibrationManagement.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers SQL Server access (Dapper + stored procedures), repositories, password hashing and JWT creation.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
        services.AddSingleton<IPasswordService>(new PasswordService());
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILookupRepository, LookupRepository>();
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IResultRepository, ResultRepository>();
        services.AddScoped<ICertificateRepository, CertificateRepository>();
        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        return services;
    }
}
