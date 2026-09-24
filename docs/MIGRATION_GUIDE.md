# Migration Guide — v1 → v2.0

This guide describes the data and code migration from the legacy ASP.NET WebForms
GiveAID v1 application to the v2.0 Clean Architecture rewrite.

## 1. Overview

| Aspect          | v1 (Legacy)                                   | v2.0                                       |
|-----------------|-----------------------------------------------|--------------------------------------------|
| Framework       | ASP.NET WebForms 4.7 + IIS Express            | ASP.NET Core 8 (WebApi + Razor + React)    |
| ORM             | EF6 + inline ADO.NET                          | EF Core 8                                  |
| Pattern         | Code-behind spaghetti                         | Clean Architecture + CQRS                  |
| Auth            | Forms auth + session                          | JWT bearer + cookie (admin)                |
| Database        | SQL Server (Express)                          | SQL Server (Express) — same instance       |
| Public site     | WebForms + jQuery                              | React 18 SPA                               |
| Admin           | WebForms admin pages                          | Razor Pages + AdminLTE                     |

The **database** is largely preserved; the **code** is fully rewritten.

## 2. Database Migration

### Tables that Map 1:1

Most v1 tables are reused with renamed columns and added audit columns:

| v1 table                  | v2 table                | Notes                                          |
|---------------------------|-------------------------|------------------------------------------------|
| `Users`                   | `Users`                 | + `CreatedAt`, `UpdatedAt`, `IsActive`         |
| `Causes`                  | `Causes`                | + `IsActive` (soft-delete)                     |
| `Campaigns`               | `Campaigns`             | + `Status` enum, `Featured` flag               |
| `Donations`               | `Donations`             | + `TransactionId`, `StripePaymentIntentId`     |
| `Galleries`               | `Galleries`             | (renamed from `GalleryItems`)                  |
| `TeamMembers`             | `TeamMembers`           |                                                |
| `Achievements`            | `Achievements`          |                                                |
| `Careers`                 | `Careers`               |                                                |
| `CareerApplications`      | `CareerApplications`    |                                                |
| `Organizations`           | `Organizations`         |                                                |
| `FAQs`                    | `Faqs`                  |                                                |
| `ContactMessages`         | `ContactMessages`       | + `Status` enum                                |
| `Conversations`           | `Conversations`         |                                                |
| `ConversationMessages`    | `ConversationMessages`  |                                                |
| `Invitations`             | `Invitations`           |                                                |
| `CmsPages`                | `CmsPages`              |                                                |

### Tables that Were Renamed

| v1                       | v2                       |
|--------------------------|--------------------------|
| `GalleryItems`           | `Galleries`              |
| `Programmes`             | *(removed — merged into Campaigns)* |

### Tables that Were Merged

**Programmes → Campaigns**: In v1, "programmes" were distinct from "campaigns". In v2.0,
they are unified under the `Campaigns` table with a `RegistrationRequired` flag. If the
field is `true`, the campaign is registration-only (no donation flow); if `false`, it's a
donation campaign.

```sql
-- v1 → v2 migration (one-time)
INSERT INTO Campaigns (Name, Description, StartDate, EndDate, Goal, Raised, Status, RegistrationRequired, CauseId)
SELECT p.Name, p.Description, p.StartDate, p.EndDate, 0, 0, 'Completed', 1, p.CauseId
FROM Programmes p
WHERE NOT EXISTS (SELECT 1 FROM Campaigns c WHERE c.Name = p.Name);
```

### New Tables (added in v2.0)

- `EmailLogs` — outbound email audit trail
- `WebhookLogs` — Stripe webhook delivery log
- `CampaignReports` — post-campaign impact reports
- `CampaignRegistrations` — user sign-ups for registration campaigns
- `BaseEntity` columns (`CreatedAt`, `UpdatedAt`) added to most existing tables

### Migration Scripts

Run in order from `database/migrations/`:

```powershell
cd database/migrations

# 1. Initial schema (idempotent — checks for table existence)
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i 001_v2_initial_schema.sql

# 2. Rename and merge tables
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i 002_rename_and_merge.sql

# 3. Add new tables and audit columns
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i 003_new_tables.sql

# 4. Seed admin user + roles
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i 004_seed_admin.sql

# 5. (Optional) Seed sample causes/campaigns
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i 005_sample_data.sql
```

## 3. Code Migration

### 3.1. Server-side (C#)

| v1 location                            | v2 location                                 | Status   |
|----------------------------------------|---------------------------------------------|----------|
| `GiveAID.Web/Controllers/*.cs`         | `src/WebApi/Controllers/*.cs`               | Rewritten |
| `GiveAID.Web/Models/EntityModels.cs`   | `src/Domain/Entities/*.cs`                  | Rewritten |
| `GiveAID.Web/Helpers/EmailService.cs`  | `src/Infrastructure/Email/SmtpEmailService.cs` | Rewritten |
| `GiveAID.Web/Helpers/HtmlSanitizer.cs` | (replaced by Ganss.XSS in Application layer)| Replaced  |
| `GiveAID.Web/Data/GiveAIDContext.cs`   | `src/Infrastructure/Persistence/GiveAIDDbContext.cs` | Rewritten |
| `GiveAID.Web/App_Start/WebApiConfig.cs`| `src/WebApi/Program.cs`                     | Rewritten |
| `Global.asax.cs`                       | *(removed — startup is `Program.cs`)*       | Removed   |

### 3.2. Client-side (React)

| v1 location                       | v2 location                                  |
|-----------------------------------|----------------------------------------------|
| `GiveAID.Client/src/api/*.js`     | `GiveAID.Client/src/services/api.js` (axios) |
| `GiveAID.Client/src/config.js`    | Updated for v2 endpoints                     |
| All page components               | Mostly preserved, services refactored        |

### 3.3. Admin Console

| v1 location                         | v2 location                          |
|-------------------------------------|--------------------------------------|
| `GiveAID.Web/admin/*.aspx`          | `src/Web/Areas/Admin/Views/**/*.cshtml` |
| `GiveAID.Web/admin/masterpages`     | `src/Web/Areas/Admin/Views/Shared/_Layout.cshtml` |

## 4. Configuration Migration

`Web.config` → `appsettings.json`:

| v1 (`Web.config`)                | v2 (`appsettings.json`)           |
|----------------------------------|-----------------------------------|
| `<connectionStrings>`            | `ConnectionStrings:DefaultConnection` |
| `<appSettings>` JWT secret       | `Jwt:Secret`                      |
| `<appSettings>` SMTP             | `Smtp:Host`, `Smtp:Port`, etc.    |
| `<appSettings>` Stripe           | `Stripe:SecretKey`, `Stripe:PublishableKey` |
| CORS *(none)*                    | `Cors:AllowedOrigins`             |
| Rate limit *(none)*              | `RateLimit:WindowSeconds`, `RateLimit:PermitLimit` |

## 5. Authentication Migration

| v1                                  | v2                                    |
|-------------------------------------|---------------------------------------|
| ASP.NET Forms Authentication         | JWT bearer (WebApi) + Cookie (Web)    |
| `FormsAuthentication.SetAuthCookie`  | `HttpContext.SignInAsync("AdminCookie")` |
| `[Authorize]` (WebForms)             | `[Authorize]` + policy (`Admin`) |
| Session-based roles                 | `Role` claim in JWT                   |

### Token Bridge for Legacy Clients

If you have v1 ASP.NET pages that need to authenticate against v2:

```csharp
// In v1 Global.asax, Application_AuthenticateRequest:
var legacyCookie = Request.Cookies[".ASPXAUTH"];
if (legacyCookie != null)
{
    var ticket = FormsAuthentication.Decrypt(legacyCookie.Value);
    var email = ticket.Name;
    // Call v2 /api/v1/auth/login with stored password to mint a v2 JWT
}
```

For most projects, a **hard cutover** is recommended: v1 is decommissioned once the React
SPA + Admin console are verified working.

## 6. Step-by-Step Cutover Plan

1. **Pre-cutover** — freeze v1 writes (or set v1 to read-only mode)
2. **Provision v2 environment** — separate database (`GiveAIDDB_v2`) to avoid touching v1
3. **Run migrations** — `001_v2_initial_schema.sql` creates an empty v2 schema
4. **Data backfill** — ETL scripts copy v1 data into v2, normalising tables (Programmes → Campaigns, etc.)
5. **Smoke test v2** — login, create campaign, donate, view admin dashboard
6. **DNS / load-balancer cutover** — point `give-aid.org` to v2 infrastructure
7. **Monitor** — watch `EmailLogs`, `WebhookLogs`, `Donations` for anomalies
8. **Decommission v1** — keep for 30 days, then archive database backup

## 7. Rollback Plan

If v2 fails in production:

1. Revert DNS to v1 (point `give-aid.org` back to v1 origin)
2. v2 databases are kept untouched for forensic analysis
3. v1 read-only flag is removed so writes resume

## 8. Post-Cutover Cleanup

- Remove `GiveAID.Web/` (legacy WebForms project) after 30-day soak
- Drop legacy tables that were merged (`Programmes`, `ProgrammePhotos`, etc.)
- Archive `Web.config` and IIS Express config in `legacy/`
- Update CI to build only the v2 solution

## 9. Common Pitfalls

1. **JWT secret mismatch** — ensure v2 `Jwt:Secret` matches what the React client was
   pointed at in `localStorage` testing. Rotating it invalidates all live tokens.
2. **CORS** — the React dev server (`localhost:3000`) must be in `Cors:AllowedOrigins`.
   Production hosts (`https://give-aid.org`) too.
3. **Image URLs** — gallery image paths stored in the DB are absolute URLs. After cutover
   to a new CDN domain, run a one-off SQL update.
4. **Stripe webhooks** — reconfigure webhook endpoint in Stripe dashboard to point at
   `/api/v1/webhooks/stripe`.
5. **Email templates** — v1 used inline HTML; v2 uses Razor templates under
   `src/Infrastructure/Email/Templates/`. Validate each template renders correctly before
   cutover.

## 10. Verification Checklist

After cutover, verify:

- [ ] `/healthz` returns 200 on both WebApi and Web
- [ ] Admin can log in with `admin@give-aid.org`
- [ ] Homepage loads with featured campaigns
- [ ] Donation flow completes end-to-end with Stripe test card `4242 4242 4242 4242`
- [ ] Email log shows the donation receipt sent
- [ ] Admin dashboard shows updated counts
- [ ] Contact form submission lands in `ContactMessages`
- [ ] FAQ and CMS pages render with custom content

If all checks pass, the migration is complete.
