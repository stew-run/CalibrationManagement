using CalibrationManagement.Api.Configuration;
using CalibrationManagement.Api.Extensions;
using CalibrationManagement.Api.Middleware;
using CalibrationManagement.Api.Provisioning;
using CalibrationManagement.Application;
using CalibrationManagement.Application.Configuration;
using CalibrationManagement.Infrastructure;
using CalibrationManagement.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Local, git-ignored settings (connection string, JWT secret). Environment variables and user-secrets still work.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

var provisioningMode = args.Contains("create-admin");
var connectionString = builder.Configuration.GetConnectionString("Default");
var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

var problems = ConfigurationValidator.Validate(connectionString, jwt, requireJwt: !provisioningMode);
if (problems.Count > 0)
{
    Console.Error.WriteLine("Configuration error - the API cannot start:");
    foreach (var problem in problems) Console.Error.WriteLine("  - " + problem);
    return 1;
}

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection(SecuritySettings.SectionName));

// Layers: Application (business services) and Infrastructure (SQL Server, repositories, password hashing, JWT creation)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString!);

// Authentication / authorization (JWT bearer + role based policies)
var jwtForValidation = new JwtSettings
{
    Issuer = jwt.Issuer,
    Audience = jwt.Audience,
    ExpiryMinutes = jwt.ExpiryMinutes,
    Key = string.IsNullOrWhiteSpace(jwt.Key) ? new string('x', ConfigurationValidator.MinJwtKeyLength) : jwt.Key // only used by 'create-admin'
};
builder.Services.AddJwtAuth(jwtForValidation);
builder.Services.AddAuthorizationPolicies();

var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5173" };
builder.Services.AddFrontendCors(origins);
builder.Services.AddLoginRateLimiting();

builder.Services.AddControllers();
builder.Services.AddSwaggerDocs();

var app = builder.Build();

if (provisioningMode)
{
    return await AdminProvisioning.RunAsync(app.Services);
}

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "no-referrer";
    if (context.Request.Path.StartsWithSegments("/api")) headers["Cache-Control"] = "no-store";
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(ServiceCollectionExtensions.CorsPolicyName);
// After CORS, so error responses (including 503 'database unavailable') still carry CORS headers and the browser can read them.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();
return 0;
