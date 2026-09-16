# Runbook

> Audience: anyone running, debugging, or deploying the project locally. Follow these steps
> in order. **Do not skip the "Verify health" step** — it catches 90% of issues.

---

## 1. Prerequisites

| Requirement | Version | How to check |
|---|---|---|
| Windows | 10/11 | `winver` |
| Node.js | 18 LTS or 20 LTS | `node -v` |
| npm | 9+ | `npm -v` |
| .NET Framework | 4.7.2 | `reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"` |
| SQL Server | Express 2019+ | `sqlcmd -S .\SQLEXPRESS -Q "SELECT @@VERSION"` |
| Visual Studio (or MSBuild) | 2019/2022 | `where.exe MSBuild` |
| IIS Express | 10+ | `where.exe iisexpress` |

---

## 2. First-time setup

```powershell
# Clone (or open existing folder)
cd "C:\Users\admin\Desktop\project NGO"

# Install frontend deps
cd GiveAID.Client
npm install
cd ..

# Build backend (validates everything compiles)
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
  GiveAID.Web\GiveAID.Web.csproj /p:Configuration=Debug
```

If MSBuild isn't at that path, find it:

```powershell
Get-ChildItem 'C:\Program Files' -Recurse -Filter MSBuild.exe |
  Where-Object { $_.FullName -match 'Visual Studio' } |
  Select-Object -First 1 -ExpandProperty FullName
```

---

## 3. Database setup

### Option A — Fresh database

```powershell
sqlcmd -S .\SQLEXPRESS -Q "IF DB_ID('GiveAIDDB') IS NULL CREATE DATABASE GiveAIDDB"
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\DB_Patch_Combined.sql"
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\NGO_Database_Causes_Restructure_Migration.sql"
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\NGO_Database_CampaignProgramme_Merge.sql"
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\NGO_Database_Invitations_Migration.sql"
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i ".\Campaigns_DataSeed.sql"
```

### Option B — Reset an existing dev DB

```sql
USE master;
GO
ALTER DATABASE GiveAIDDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
DROP DATABASE GiveAIDDB;
GO
CREATE DATABASE GiveAIDDB;
GO
```

Then re-run Option A.

---

## 4. Running the project

### Both at once (recommended)

```powershell
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
npm start
```

This uses `concurrently` to:
1. Run `start-backend.ps1` (kills stale port, launches IIS Express for the .NET API)
2. Run `start-frontend.ps1` (kills stale port, launches `react-scripts start`)
3. Open the frontend at <http://localhost:3000>
4. Backend API at <http://localhost:44300> (or whatever `Web.config` → `applicationhost.config` says)

### Run them separately

```powershell
# Terminal 1
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
npm run start:backend

# Terminal 2
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
npm run start:frontend
```

### Stop everything

```powershell
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
npm run stop
```

---

## 5. Verify health

After `npm start` is up:

```powershell
# Backend health (should return 200 with "Healthy")
curl http://localhost:44300/api/health

# Frontend (should load the home page)
Start-Process http://localhost:3000
```

If the backend returns `Unable to connect`, check that IIS Express is bound to port 44300
(see `GiveAID.Web/.vs/GiveAID.Web/config/applicationhost.config`). If you change the port,
also update:
- `GiveAID.Client/package.json` → `proxy`
- `GiveAID.Client/.env.development.local` → `REACT_APP_API_BASE_URL`
- `GiveAID.Web/Web.config` → any hardcoded origin

---

## 6. Demo accounts

```
SuperAdmin →  admin@give-aid.org   /  Admin@123
User       →  user@example.com     /  User@123
```

If `Admin@123` doesn't work (e.g., after a DB reset):

```powershell
# Login as SuperAdmin from another browser first, then:
$token = "..."   # your JWT
Invoke-RestMethod -Method POST `
  -Uri "http://localhost:44300/api/auth/bootstrap" `
  -Headers @{ Authorization = "Bearer $token" }
```

Or, simplest: drop and recreate the DB (Option B above). `Global.asax.SeedDatabase()` will
re-create both accounts.

---

## 7. Debugging checklist

### "Network Error" in browser console

1. Is the backend running? `curl http://localhost:44300/api/health`
2. Is the proxy set correctly in `package.json`?
3. Is CORS configured? Check `WebApiConfig.cs` origins list.

### Login returns "Invalid email or password"

1. Confirm the user exists in DB: `SELECT * FROM Users WHERE email = 'admin@give-aid.org'`
2. Confirm `is_active = 1`.
3. Reset via `/api/auth/bootstrap` (SuperAdmin JWT required).

### Login returns 401 immediately

JWT secret mismatch between sessions (very rare — happens if `Web.config` `JwtSecret` changes
mid-session). Clear `localStorage.GiveAID_token` and reload.

### ESLint complains

```powershell
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
npx eslint src/ --ext .js,.jsx --max-warnings=0
```

Current baseline: **0 errors, 0 warnings**. Any deviation indicates a regression.

### Backend won't build

1. Run MSBuild (path above) and look for `CSxxxx` errors.
2. Most common cause: missing `using` directive. Add it.
3. Less common: stale `bin/` folder. Delete `GiveAID.Web/bin/` and `obj/` and rebuild.

### "Cannot find module" on backend

`packages/` folder is missing. Run:

```powershell
nuget restore GiveAID.Web\GiveAID.Web.csproj
```

### Frontend won't start

```powershell
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
Remove-Item -Recurse -Force node_modules
Remove-Item -Force package-lock.json
npm install
```

---

## 8. Logging

- Backend: `System.Diagnostics.Debug.WriteLine` (visible in VS Output window during debug;
  also `Global.asax` uses it for `[Startup] SeedDatabase failed: ...`).
- Frontend: `console.log` + browser DevTools.
- IIS Express logs: `GiveAID.Web/.vs/.../logs/` (only when running under VS).

For production: replace `EmailService` with real SMTP and add structured logging (Serilog,
NLog) — not in scope for this dev project.

---

## 9. Production deployment checklist

When moving from dev to staging/prod, **review every item**:

- [ ] `Web.config`: replace `JwtSecret` with a long random string (≥32 chars). Do NOT reuse
      the dev value.
- [ ] `Web.config`: set `<customErrors mode="On" />` and `<httpRuntime targetFramework="4.7.2" />`.
- [ ] `Web.config`: update `Cors:AllowedOrigins` to the production frontend domain(s).
- [ ] `Web.config`: update connection string to a production SQL Server (Integrated Security
      → SQL auth recommended for prod).
- [ ] `Global.asax.cs`: switch from debug-mode seed to a one-time migration job.
- [ ] `WebApiConfig.cs`: update `EnableCorsAttribute` origins.
- [ ] `EmailService`: replace mock SMTP with real provider (SendGrid, SES, etc.).
- [ ] `npm run build` in `GiveAID.Client/` → deploy `build/` to a CDN or static host.
- [ ] Update the React app's `REACT_APP_API_BASE_URL` env var to the production API URL.
- [ ] HTTPS everywhere — IIS Express in dev uses HTTP; production must be HTTPS-only.
- [ ] Set up DB backups + run all `*.sql` migrations on the prod DB before deploying the API.

---

## 10. Common tasks

### Add a new admin page

1. Create `src/pages/admin/AdminXxxPage.js`. Wrap in `<AdminPageFrame>`.
2. Add a route in `src/App.js` under the admin section.
3. Done.

### Add a new API endpoint

1. Add action method to the relevant `Controllers/XxxController.cs`.
2. Add the DTO in `Models/ViewModels.cs`.
3. Add a method to the matching `services/index.js` export.
4. Add the URL to `config.js → API_ENDPOINTS`.
5. Restart the backend.

### Add a new DB column

1. Write a `*.sql` migration in the project root (idempotent!).
2. Add the property to the matching entity in `EntityModels.cs`.
3. Rebuild backend.
4. Apply the SQL migration to the DB.

### Reset everything

```powershell
# Stop both servers
cd "C:\Users\admin\Desktop\project NGO\GiveAID.Client"
npm run stop

# Drop and recreate DB (see Section 3 Option B)

# Rebuild backend
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
  GiveAID.Web\GiveAID.Web.csproj /p:Configuration=Debug

# Restart
npm start
```
