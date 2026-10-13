using System.Threading.RateLimiting;
using CalibrationManagement.Api.Authorization;
using CalibrationManagement.Domain.Constants;
using CalibrationManagement.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CalibrationManagement.Api.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>JWT bearer authentication. Token validation rules live in <see cref="JwtTokenService.BuildValidationParameters"/>.</summary>
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, JwtSettings jwt)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = JwtTokenService.BuildValidationParameters(jwt);
                options.RequireHttpsMetadata = true;
                options.SaveToken = false;
            });
        return services;
    }

    /// <summary>Role based policies: Read, Write, Manage, AdminOnly.</summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyNames.Read, p => p.RequireAuthenticatedUser());
            options.AddPolicy(PolicyNames.Write, p => p.RequireRole(RoleNames.Administrator, RoleNames.QualityManager, RoleNames.Technician));
            options.AddPolicy(PolicyNames.Manage, p => p.RequireRole(RoleNames.Administrator, RoleNames.QualityManager));
            options.AddPolicy(PolicyNames.AdminOnly, p => p.RequireRole(RoleNames.Administrator));
        });
        return services;
    }

    /// <summary>CORS: only the configured front-end origins.</summary>
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, string[] origins)
    {
        services.AddCors(options => options.AddPolicy(CorsPolicyName, policy => policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Content-Disposition")));
        return services;
    }

    public const string CorsPolicyName = "Frontend";

    /// <summary>Throttles credential guessing on the login endpoint (10 requests / minute / IP).</summary>
    public static IServiceCollection AddLoginRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
        });
        return services;
    }
}
