# Runbook — GiveAID v2.0

> Audience: anyone running, debugging, or deploying the project. Follow these steps in
> order. **Don't skip "Verify health"** — it catches 90% of issues.

## 1. Prerequisites

| Requirement       | Version            | How to check                              |
|-------------------|--------------------|-------------------------------------------|
| Windows           | 10/11              | `winver`                                  |
| .NET SDK          | 8.0+               | `dotnet --version`                        |
| Node.js           | 18 LTS or 20 LTS   | `node -v`                                 |
| npm               | 9+                 | `npm -v`                                  |
| SQL Server        | Express 2019+      | `sqlcmd -S .\SQLEXPRESS -Q "SELECT @@VERSION"` |
| Git               | 2.40+              | `git --version`                           |
| Visual Studio / Rider / VS Code | 2022 17.8+ / 2024.1+ | `where.exe devenv`            |

## 2. First-time Setup

### 2.1. Database

```powershell
# Create database (idempotent — skip if already exists)
sqlcmd -S .\SQLEXPRESS -Q "IF DB_ID('GiveAIDDB') IS NULL CREATE DATABASE GiveAIDDB"

# Apply EF Core migrations
# <REPO_ROOT> = the folder containing this file (the repo root)
cd "<REPO_ROOT>"
dotnet ef database update --project src/Infrastructure/GiveAID.V2.Infrastructure.csproj --startup-project src/WebApi/GiveAID.V2.WebApi.csproj
```

The migration also seeds the default `Admin` user.

### 2.2. Backend

```powershell
cd "<REPO_ROOT>"
dotnet restore GiveAID.V2.slnx
dotnet build GiveAID.V2.slnx
# Expected: Build succeeded. 0 Warning(s) 0 Error(s)
```

### 2.3. Frontend

```powershell
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
npm install --no-audit --no-fund
```

## 3. Run (Three Terminals)

### Terminal 1 — WebApi

```powershell
cd "<REPO_ROOT>"
dotnet run --project src/WebApi/GiveAID.V2.WebApi.csproj
# Listens on http://localhost:5231
```

### Terminal 2 — Admin Console

```powershell
cd "<REPO_ROOT>"
dotnet run --project src/Web/GiveAID.V2.Web.csproj
# Listens on http://localhost:5069
```

### Terminal 3 — React Client

```powershell
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
npm start
# Listens on http://localhost:3000
```

## 4. Verify Health

```powershell
curl.exe -s http://localhost:5231/healthz
# Expected: 200 OK (empty body)

curl.exe -s http://localhost:5231/api/v1/health
# Expected: {"status":"healthy","version":"2.0",...}

curl.exe -s http://localhost:5069/Admin/Auth/Login
# Expected: 200 OK (HTML login page)

curl.exe -s http://localhost:3000/api/v1/health
# Expected: same as 5231 (proxy works)
```

If any of these fail, jump to **Troubleshooting** below.

## 5. Default Credentials

| Role   | Email                  | Password    |
|--------|------------------------|-------------|
| Admin  | admin@give-aid.org     | Admin@123   |
| User   | user@give-aid.org      | User@123    |

> **Change these in production.** See `src/Infrastructure/Persistence/Seed/DatabaseSeeder.cs`.

## 6. Useful URLs

| Service                | URL                                          |
|------------------------|----------------------------------------------|
| WebApi root            | http://localhost:5231                        |
| OpenAPI (Scalar UI)    | http://localhost:5231/scalar/v1              |
| Health check           | http://localhost:5231/healthz                |
| Admin login            | http://localhost:5069/Admin/Auth/Login       |
| Admin dashboard        | http://localhost:5069/Admin/Dashboard        |
| React site             | http://localhost:3000                        |
| React login            | http://localhost:3000/login                  |

## 7. Smoke Test Script

After everything is up, run this in PowerShell:

```powershell
# Backend health
$health = curl.exe -s http://localhost:5231/api/v1/health | ConvertFrom-Json
Write-Host "Backend: $($health.status) v$($health.version)"

# Admin login page renders
$login = curl.exe -s -o $null -w "%{http_code}" http://localhost:5069/Admin/Auth/Login
Write-Host "Admin login page: $login"

# React homepage loads
$home = curl.exe -s -o $null -w "%{http_code}" http://localhost:3000
Write-Host "React homepage: $home"

# Run all tests
cd "C:\Users\admin\Desktop\project NGO.v2"
dotnet test GiveAID.V2.slnx --no-build --logger "console;verbosity=quiet"
```

Expected:
- `Backend: healthy v2.0`
- `Admin login page: 200`
- `React homepage: 200`
- `Passed!  - Failed: 0, Passed: 169, Total: 169`

## 8. Troubleshooting

### 8.1. WebApi fails to start with "address already in use"

```powershell
# Find and kill the process
Get-NetTCPConnection -LocalPort 5231 | Select-Object OwningProcess
Stop-Process -Id <pid> -Force
```

### 8.2. Database connection error

1. Confirm SQL Server is running: `Get-Service MSSQLSERVER` or `Get-Service MSSQL$SQLEXPRESS`
2. Test connection: `sqlcmd -S .\SQLEXPRESS -Q "SELECT 1"`
3. If using localdb, switch connection string in `appsettings.json`

### 8.3. EF migration fails with "pending model changes"

```powershell
dotnet ef migrations add FixPendingChanges --project src/Infrastructure/GiveAID.V2.Infrastructure.csproj --startup-project src/WebApi/GiveAID.V2.WebApi.csproj
dotnet ef database update --project src/Infrastructure/GiveAID.V2.Infrastructure.csproj --startup-project src/WebApi/GiveAID.V2.WebApi.csproj
```

### 8.4. Admin console shows "Cannot connect to API"

- Verify WebApi is running on port 5231: `curl http://localhost:5231/healthz`
- Verify `src/Web/appsettings.json` has `"Api:BaseUrl": "http://localhost:5231"`
- Verify CORS in WebApi allows the admin origin (if served from another host)

### 8.5. React client shows blank page

1. Check browser dev tools console for errors
2. Verify React dev server compiled: terminal should show "webpack compiled successfully"
3. Hard-reload: Ctrl+Shift+R
4. Clear localStorage: dev tools → Application → Storage → Clear site data

### 8.6. 401 Unauthorized on API calls

- Token expired (default 60 min) — re-login
- JWT secret rotated — users must re-login
- Check `[Authorize]` attribute on the endpoint

### 8.7. Tests fail with database errors

The functional tests use **InMemory DB** — no SQL Server needed. If they fail:

```powershell
# Confirm tests run
dotnet test tests/WebApi.FunctionalTests/GiveAID.V2.WebApi.FunctionalTests.csproj
```

If only specific tests fail, read the test output for the actual exception.

### 8.8. SMTP "5.7.0 Authentication Required"

- Gmail blocks plain auth — use an **App Password** (https://support.google.com/accounts/answer/185833)
- Other providers may require different ports or SSL settings

### 8.9. Stripe webhook signature validation fails

- Local dev: use Stripe CLI to forward webhooks:
  ```bash
  stripe listen --forward-to http://localhost:5231/api/v1/webhooks/stripe
  ```
- Copy the printed `whsec_...` into `appsettings.Development.json`

## 9. Reset to a Clean State

```powershell
# Drop and recreate database
sqlcmd -S .\SQLEXPRESS -Q "DROP DATABASE IF EXISTS GiveAIDDB"
sqlcmd -S .\SQLEXPRESS -Q "CREATE DATABASE GiveAIDDB"

# Re-apply migrations (also re-seeds)
cd "C:\Users\admin\Desktop\project NGO.v2"
dotnet ef database update --project src/Infrastructure/GiveAID.V2.Infrastructure.csproj --startup-project src/WebApi/GiveAID.V2.WebApi.csproj

# Stop everything
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "node"    -ErrorAction SilentlyContinue | Stop-Process -Force
```

## 10. Performance Profiling

```powershell
# CPU sampling
dotnet trace collect --process-id <pid> --duration 00:00:30 --profile cpu-sampling

# Live metrics
dotnet-counters monitor --process-id <pid> --refresh-interval 1
```

EF Core SQL logging:

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

## 11. Where to Look

| Symptom                                  | Look at                                           |
|------------------------------------------|---------------------------------------------------|
| App won't start                          | `src/WebApi/Program.cs` — DI registration order   |
| Wrong DB column type                     | `src/Infrastructure/Persistence/Configurations/`  |
| 500 on endpoint                          | Response body — `errors` field has details        |
| Login fails                              | `EmailLogs` table — any send failures              |
| Stripe payments failing                  | `WebhookLogs` table — error column                |
| Admin 401                                | Browser cookie `GiveAID.Admin` — check expiry     |
| Tests flaky                              | Look for `Thread.Sleep` or shared static state    |

## 12. Environment Configuration (CRITICAL)

The application REQUIRES `ConnectionStrings:DefaultConnection` to be set for each
environment. The default `appsettings.json` contains a placeholder — the app will
**fail to start** if the connection string is missing.

### How to configure per environment

| Environment | File | How to populate |
|-------------|------|-----------------|
| Development | `src/WebApi/appsettings.Development.json` | Edit locally, file is gitignored |
| Staging | `src/WebApi/appsettings.Staging.json` | Edit on server, file is gitignored |
| Production | `src/WebApi/appsettings.Production.json` | Edit on server, file is gitignored |
| Any (preferred) | Environment variable | `ConnectionStrings__DefaultConnection=Server=...` |

### Fail-fast on missing config

If `ConnectionStrings:DefaultConnection` is empty or placeholder when the app starts,
it will throw an exception during dependency injection setup. This is INTENTIONAL —
better to fail at startup than silently corrupt data.

### DO NOT

- Commit real connection strings (passwords) to git history
- Use SQL Authentication in production without first rotating any leaked credentials
- Copy the Development connection string to Staging/Production — they should be different servers/databases

## 13. Contact

- **Slack**: `#giveaid-dev` channel
- **Email**: tech-lead@give-aid.org
- **On-call rota**: see internal wiki

---

For production deployment, see [DEPLOYMENT.md](DEPLOYMENT.md).
For API contracts, see [API_REFERENCE.md](API_REFERENCE.md).
For architecture details, see [ARCHITECTURE.md](ARCHITECTURE.md).
