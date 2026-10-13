# Calibration Management – Backend (ASP.NET Core 8 Web API)

Layered solution. Dependencies point inwards: `Api → Application → Domain`, and `Infrastructure → Application + Domain`. Domain depends on nothing.

```
backend/
├── CalibrationManagement.sln
├── src/
│   ├── CalibrationManagement.Api            Controllers, middleware, Program.cs, DI/auth/CORS/Swagger extensions, create-admin command
│   ├── CalibrationManagement.Application    DTOs, service + repository interfaces, business services, Guard validation, exceptions, paging
│   ├── CalibrationManagement.Domain         Entities (User) and constants (RoleNames)
│   └── CalibrationManagement.Infrastructure SQL Server access (Dapper, stored procedures only), repositories, BCrypt password service, JWT creation
└── tests/CalibrationManagement.Tests        xUnit tests
```

Cross-cutting: JWT bearer auth, role policies (`Read`, `Write`, `Manage`, `AdminOnly`), `ExceptionHandlingMiddleware` (RFC 7807 problem+json), login rate limiting, CORS for the configured origins, Swagger (Development only).

```powershell
cd src\CalibrationManagement.Api
Copy-Item appsettings.Local.json.example appsettings.Local.json   # then edit: connection string + Jwt:Key
dotnet run -- create-admin                                       # one-time: prompts for the administrator, stores only a BCrypt hash
dotnet run --launch-profile https                                # https://localhost:7001  (Swagger: /swagger)
cd ..\..; dotnet test                                            # xUnit tests
```

`appsettings.Local.json` is git-ignored. Secrets can alternatively be set with `dotnet user-secrets set "Jwt:Key" "<secret>"` from the API project folder.

Where things moved from the old single-project layout: `Controllers/*` → Api/Controllers (one file per controller); `Services/*` → Application/Services (business) and Infrastructure/Security (`PasswordService`, `JwtTokenService`); `Repositories/*`, `Data/*` → Infrastructure; `Interfaces/*`, `DTOs/*`, `Common/*` → Application; `UserRecord` → `Domain/Entities/User`; `RoleNames` → Domain/Constants; `PolicyNames` → Api/Authorization; `JwtSettings` → Infrastructure/Security; `SecuritySettings` → Application/Configuration.
