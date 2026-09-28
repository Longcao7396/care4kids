# Deployment & Runbook — GiveAID v2.0

This document covers local development, production deployment, and operational procedures
for GiveAID v2.0.

## 1. Local Development

### Prerequisites

| Tool           | Version | Notes                                         |
|----------------|---------|-----------------------------------------------|
| .NET SDK       | 8.0+    | `dotnet --version` should report 8.x          |
| Node.js        | 18+     | LTS                                           |
| SQL Server     | LocalDB (ships with VS) or Express 2019+ | Default = `(localdb)\MSSQLLocalDB` |
| Git            | 2.40+   |                                               |
| Visual Studio  | 2022 17.8+ / Rider 2024.1+ / VS Code | C# extension |

### Setup

```powershell
# Clone
git clone <repo-url>
cd "project-NGO"

# Verify the database connection (canonical instance is (localdb)\MSSQLLocalDB)
powershell -ExecutionPolicy Bypass -File verify-database.ps1

# Apply database schema + seeds
powershell -ExecutionPolicy Bypass -File database\99_Apply-All.ps1

# Backend
dotnet restore GiveAID.V2.slnx
dotnet build GiveAID.V2.slnx

# Frontend
cd GiveAID.Client
npm install
```

### Run (2 terminals or use START.bat)

**Terminal 1 — WebApi (port 5231)**
```powershell
dotnet run --project src/WebApi/GiveAID.V2.WebApi.csproj
```

**Terminal 2 — React client (port 3000)**
```powershell
cd GiveAID.Client
npm start
```

**Or use the launcher:**
```powershell
START.bat
```

### Default ports

| Service        | URL                                   |
|----------------|---------------------------------------|
| WebApi         | http://localhost:5231                 |
| Scalar OpenAPI | http://localhost:5231/scalar/v1       |
| React client   | http://localhost:3000                 |
| React admin    | http://localhost:3000/admin           |

## 2. Configuration Reference

### `src/WebApi/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=GiveAIDDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connect Timeout=15",
  },
  "Jwt": {
    "Secret": "<64-byte secret>",
    "Issuer": "GiveAID.V2",
    "Audience": "GiveAID.V2.Client",
    "ExpiryMinutes": 60
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "noreply@give-aid.org",
    "Password": "<app password>"
  },
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_..."
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:3000,https://give-aid.org"
  },
  "RateLimit": {
    "WindowSeconds": 60,
    "PermitLimit": 100
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### Production secrets

Never commit secrets. Use **environment variables** or a secret store:

```powershell
$env:Jwt__Secret = "<prod-secret>"
$env:Stripe__SecretKey = "<prod-key>"
$env:ConnectionStrings__DefaultConnection = "Server=prod-sql;..."
```

Or in IIS `web.config`:

```xml
<environmentVariables>
  <add name="Jwt__Secret" value="..." />
</environmentVariables>
```

## 3. Production Deployment

### Option A — IIS (recommended for Windows Server)

```powershell
# 1. Publish
dotnet publish src/WebApi/GiveAID.V2.WebApi.csproj -c Release -o publish/api
dotnet publish src/Web/GiveAID.V2.Web.csproj     -c Release -o publish/web

# 2. Install ASP.NET Core Hosting Bundle
# https://dotnet.microsoft.com/download/dotnet/8.0
# (Includes ASP.NET Core Module for IIS)

# 3. Create IIS sites
New-WebSite -Name "GiveAID.Api"  -Port 80  -PhysicalPath "C:\inetpub\giveaid-api"  -ApplicationPool "GiveAID.Api.AppPool"
New-WebSite -Name "GiveAID.Web"  -Port 81  -PhysicalPath "C:\inetpub\giveaid-web"  -ApplicationPool "GiveAID.Web.AppPool"

# 4. React client — build and deploy to CDN/S3 or static IIS site
cd GiveAID.Client
npm run build
# Upload build/ to your static host
```

### Option B — Linux + Nginx + systemd

```bash
# Build
dotnet publish src/WebApi/GiveAID.V2.WebApi.csproj -c Release -o /var/www/giveaid-api

# systemd unit: /etc/systemd/system/giveaid-api.service
[Unit]
Description=GiveAID API
After=network.target

[Service]
WorkingDirectory=/var/www/giveaid-api
ExecStart=/usr/bin/dotnet /var/www/giveaid-api/GiveAID.V2.WebApi.dll
Restart=always
User=www-data
Environment=ASPNETCORE_URLS=http://0.0.0.0:5231
Environment=Jwt__Secret=<secret>

[Install]
WantedBy=multi-user.target
```

```nginx
# /etc/nginx/sites-available/giveaid.org
server {
    listen 443 ssl http2;
    server_name api.give-aid.org;
    ssl_certificate     /etc/letsencrypt/live/give-aid.org/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/give-aid.org/privkey.pem;

    location / {
        proxy_pass         http://localhost:5231;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

### Database migrations on production

```bash
# Apply EF Core migrations (preferred for v2 — incremental)
dotnet ef database update --project src/Infrastructure/GiveAID.V2.Infrastructure.csproj --startup-project src/WebApi/GiveAID.V2.WebApi.csproj

# Or run raw SQL migrations (when v1 legacy DB needs bridge)
sqlcmd -S prod-sql -d GiveAIDDB -i database/migrations/001_v2_initial_schema.sql
```

## 4. Stripe Configuration

1. **Stripe Dashboard** → Developers → Webhooks → Add endpoint:
   - URL: `https://api.give-aid.org/api/v1/webhooks/stripe`
   - Events: `payment_intent.succeeded`, `payment_intent.payment_failed`, `charge.refunded`
2. Copy the signing secret into `Stripe:WebhookSecret`
3. Test with Stripe CLI:
   ```bash
   stripe listen --forward-to https://api.give-aid.org/api/v1/webhooks/stripe
   stripe trigger payment_intent.succeeded
   ```

## 5. SMTP Configuration

For Gmail (development):
1. Enable 2FA on the account
2. Create an [App Password](https://support.google.com/accounts/answer/185833)
3. Use the app password as `Smtp:Password`

For production, use a transactional email provider (SendGrid, Postmark, Mailgun).

## 6. Operational Runbook

### Health checks

- `GET /healthz` — used by load balancer (200 OK, no body)
- `GET /api/v1/health` — versioned, returns JSON with version

### Logs

- **Structured logging** via `ILogger<T>` — JSON in production
- Default sinks: Console + Debug
- For centralised logging, plug in Serilog → Seq / Elasticsearch / Application Insights

### Database backups

```powershell
# Daily full backup
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "BACKUP DATABASE [GiveAIDDB] TO DISK='D:\Backups\GiveAIDDB_Full.bak' WITH INIT"

# Hourly differential
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "BACKUP DATABASE [GiveAIDDB] TO DISK='D:\Backups\GiveAIDDB_Diff.bak' WITH DIFFERENTIAL"
```

Restore:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "RESTORE DATABASE [GiveAIDDB] FROM DISK='D:\Backups\GiveAIDDB_Full.bak' WITH REPLACE"
```

### Common incidents

#### High error rate on /api/v1/donations

1. Check `WebhookLogs` — is Stripe reaching us?
2. Check `EmailLogs` — are donation receipts being sent?
3. Verify `Stripe:SecretKey` is valid (production key, not test)
4. Check rate-limit headers — if many 429s, raise `RateLimit:PermitLimit`

#### JWT 401s after deploy

1. Confirm `Jwt:Secret` did **not** change (rotating invalidates all live tokens)
2. Confirm issuer/audience match between WebApi and any other JWT consumers

#### Admin console won't load

1. Check WebApi health — admin talks to it via `Api:BaseUrl`
2. Verify CORS allows the admin origin (if served from a different domain)
3. Clear browser cookies and re-login

### Monitoring recommendations

- **Uptime**: UptimeRobot / Pingdom on `/healthz`
- **APM**: Application Insights or Datadog on the WebApi
- **Logs**: Seq / Elasticsearch — search for `LogLevel:Error`
- **Errors**: Sentry — catch unhandled exceptions in WebApi

## 7. Disaster Recovery

| Scenario                   | Recovery time | Action                                      |
|----------------------------|---------------|---------------------------------------------|
| WebApi instance crash      | < 1 min       | systemd / IIS auto-restart                  |
| Database corruption        | 1–2 hours     | Restore from latest backup                  |
| Stripe webhook outage       | < 1 hour      | Stripe retries automatically; replay missed |
| Full region outage         | < 4 hours     | Failover to secondary region + DB replica   |

## 8. Performance Targets

| Metric                  | Target       |
|-------------------------|--------------|
| `/healthz` p95 latency  | < 50 ms      |
| `/api/v1/causes` p95    | < 200 ms     |
| `/api/v1/campaigns` p95 | < 300 ms    |
| Donation create p95     | < 500 ms     |
| Concurrent users        | ≥ 500        |

If a metric degrades:
1. Check EF Core query performance — add indexes via new migration
2. Enable response caching via `[ResponseCache]` for read-heavy endpoints
3. Profile with `dotnet-trace` / `dotnet-counters`

## 9. Maintenance Windows

Recommended: **Tuesday 02:00–04:00 UTC** for:
- Database index rebuilds
- EF Core migrations
- Dependency upgrades

Notify admins 48 hours in advance via the admin dashboard banner.

## 10. Support Contacts

| Role           | Contact                |
|----------------|------------------------|
| Tech lead      | tech-lead@give-aid.org |
| DBA            | dba@give-aid.org       |
| DevOps         | devops@give-aid.org    |
| Stripe support | https://support.stripe.com |
