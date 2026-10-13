# Calibration Management

ASP.NET Core 8 Web API (JWT) + React 18 (Vite) + SQL Server.

```
CalibrationManagement/
├── backend/      ASP.NET Core solution, 4 layers + tests (see backend/README.md)
├── frontend/     React + Vite app (see frontend/README.md)
├── database/     SQL Server scripts, run in order (see database/README.md)
├── .gitignore
└── README.md
```

## First run

1. **Database** – run the scripts in `database/` in order (`01` … `07`).
2. **Backend**
   ```powershell
   cd backend\src\CalibrationManagement.Api
   Copy-Item appsettings.Local.json.example appsettings.Local.json   # set the connection string + Jwt:Key
   dotnet run -- create-admin                                       # one-time: creates the administrator (stores only a BCrypt hash)
   dotnet run --launch-profile https                                # https://localhost:7001  (Swagger: /swagger)
   ```
3. **Frontend**
   ```powershell
   cd frontend
   npm install
   npm run dev                                                      # http://localhost:5173
   ```
   `frontend/.env` holds `VITE_API_BASE_URL` (default `https://localhost:7001/api`).

Tests: `cd backend; dotnet test`.
