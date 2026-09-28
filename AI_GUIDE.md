# AI_GUIDE.md — How AI Assistants Should Work With This Codebase

> **Audience**: AI assistants (Cursor, Claude Code, GitHub Copilot, ChatGPT, etc.) that will be asked to upgrade, refactor, or debug this project in the future.
>
> **Purpose**: Prevent the AI from re-introducing bugs we've already fixed, breaking architectural invariants, or guessing when authoritative sources exist.

This document is the **single source of truth** for non-obvious decisions. Read it BEFORE making any non-trivial change.

---

## 1. Project Identity

- **Name**: GiveAID V2 — NGO donation platform
- **Version**: 2.0.0 (Clean Architecture rewrite, Sept 2026)
- **Domain**: Charity donation website (Campaigns, Causes, Donations, Stripe payments)
- **Stack**:
  - **Frontend**: React 18 (CRA), React Router 6, React Bootstrap 2, Axios
  - **Backend**: .NET 10, ASP.NET Core WebApi, MediatR (CQRS), FluentValidation, EF Core 10, BCrypt.Net
  - **Database**: SQL Server (LocalDB in dev, configurable for prod)
  - **Payments**: Stripe (mock gateway in dev)
  - **Auth**: JWT Bearer + BCrypt password hashing
  - **Data Protection**: Soft delete pattern with global query filters
  - **Compliance**: Automatic audit logging for all entity changes

---

## 2. Repository Layout

```
project NGO.v2/
├── src/                          # Backend (.NET, Clean Architecture)
│   ├── Domain/                   # Entities, value objects, domain exceptions
│   │   └── Entities/             # User, Campaign, Cause, Donation, AuditLog, etc.
│   ├── Application/              # Use cases (CQRS via MediatR)
│   │   ├── Common/Interfaces/    # IApplicationDbContext, IPasswordHasher, EmailOptions
│   │   ├── Common/Behaviors/     # ValidationBehavior (global FluentValidation pipeline)
│   │   ├── Features/             # Auth/, Campaigns/, Causes/, Donations/, etc.
│   │   │   └── Auth/Commands/Login/LoginCommandHandler.cs  ← example
│   │   └── Services/             # ApplicationServiceCollectionExtensions
│   ├── Infrastructure/           # External concerns
│   │   ├── Persistence/          # EF Core DbContext, migrations, Seed/
│   │   │   ├── Configurations/   # AuditLogConfiguration, UserConfiguration, etc.
│   │   │   └── Migrations/       # EF Core migrations including audit log table
│   │   ├── Security/             # BCrypt PasswordHasher, JwtTokenService
│   │   ├── Email/                # SMTP, email sender
│   │   ├── Payments/             # Stripe gateway + Mock
│   │   └── Services/             # DI registration
│   └── WebApi/                   # HTTP layer
│       ├── Controllers/          # AuthController, CampaignsController, etc.
│       ├── Middleware/           # ExceptionHandlingMiddleware, RateLimit
│       ├── Program.cs            # ← service registration order matters
│       └── appsettings*.json     # ← DEV config; .Production/.Development gitignored
│
├── GiveAID.Client/               # Frontend (React)
│   ├── src/
│   │   ├── pages/                # Public pages + admin/ subfolder (25 admin pages)
│   │   ├── components/           # Shared UI
│   │   ├── contexts/             # AuthContext.js ← single auth state owner
│   │   ├── services/             # api.js (axios + interceptor), authService.js
│   │   └── config.js             # API_BASE_URL, STORAGE_KEYS
│   ├── scripts/                  # PowerShell scripts (start-backend, start-frontend)
│   └── package.json              # npm start = prestart → backend + frontend
│
├── docs/                         # User-facing documentation
│   ├── ARCHITECTURE.md           # Layered design + diagrams
│   ├── API_REFERENCE.md          # All endpoints
│   ├── DATABASE.md               # Schema + ERD
│   ├── STRIPE_PRODUCTION_SETUP.md
│   └── STRIPE_INTEGRATION_TEST_REPORT.md
│
├── START.bat                     # One-click dev startup (sets env vars)
├── STOP.bat                      # Kills backend + frontend
├── package-for-distribution.ps1  # Creates .zip for non-technical testers
├── HUONG_DAN.txt                 # Vietnamese guide for non-tech testers
├── AI_GUIDE.md                   # THIS FILE
└── AI_WARNING.md                 # ← READ THIS TOO before editing
```

---

## 3. Architectural Invariants (DO NOT BREAK)

### 3.1 Clean Architecture layering (Backend)

```
Domain  ←  Application  ←  Infrastructure
                  ↑
                WebApi
```

- **Domain** has NO dependencies on other projects. Pure C# entities + exceptions.
- **Application** depends on Domain only. All business logic here.
- **Infrastructure** depends on Application. Implements interfaces (IPasswordHasher, IEmailSender, etc.).
- **WebApi** depends on Application + Infrastructure. HTTP + DI wiring.

**Violation pattern to flag**: If you see `using GiveAID.Infrastructure` inside `Application/`, that's wrong. Refactor it out before continuing.

### 3.2 CQRS via MediatR

- Every use case = 1 Command/Query + 1 Handler. No fat services.
- Handlers are registered by assembly scan in `ApplicationServiceCollectionExtensions.AddApplicationServices()`.
- **ValidationBehavior is NOW REGISTERED** — FluentValidation runs automatically for all MediatR requests. Handlers no longer need to manually call `validator.ValidateAsync()`.

### 3.3 Soft Delete Pattern

- All entities inherit from `BaseEntity` which includes `IsDeleted` and `DeletedAt` fields.
- **Global Query Filter**: EF Core automatically filters `WHERE IsDeleted = false` on all queries.
- **Automatic Soft Delete**: When `context.Remove()` is called, `SaveChangesAsync` intercepts it and converts to soft delete (`IsDeleted = true`, `DeletedAt = DateTime.UtcNow`).
- To query deleted entities: use `IgnoreQueryFilters()` in LINQ.

### 3.4 Audit Log MVP

- **Automatic tracking**: Every Create/Update/Delete operation on `BaseEntity` is logged to `audit_logs` table.
- **JSON snapshots**: Before/after values captured via EF Core's `PropertyValues`.
- **Who/When**: Tracks `UserId` (from `ICurrentUserService`), `Timestamp`, `EntityType`, `EntityId`.
- Implementation: `GiveAIDDbContext.SaveChangesAsync()` captures changes before saving.

### 3.5 Auth flow

```
Frontend                    Backend
─────────                   ────────
LoginPage.js  ──POST──→  AuthController.Login
                              ↓
                        LoginCommandHandler
                              ↓
                        BCrypt.Verify(password, user.PasswordHash)
                              ↓
                        If OK → JwtTokenService.Issue(userId, role)
                              ↓
                        200 { success, data: { token, userId, ... } }
```

**Critical**: `LoginCommandHandler` checks `IsVerified` BEFORE password. Email verification is a hard requirement (configurable via `Email:RequireVerification` — `false` in dev).

### 3.6 Frontend service layer

```
LoginPage.js
   ↓ login({ username, password })
AuthContext.login()
   ↓ authService.login() throws on failure with err.response.data
authService.login()
   ↓ api.post('/auth/login', credentials)
api.js interceptor
   - Success: unwraps { success, message, data } → returns body.data
   - Failure: rejects with err.response.data preserved
```

**Do NOT swallow errors in service layer.** AuthContext depends on the error message bubbling up.

---

## 4. Configuration

### 4.1 Backend env vars (REQUIRED in production)

| Env var | Required? | Purpose |
|---|---|---|
| `Jwt__Secret` | YES (prod) | JWT signing key. Min 32 chars. Throws on startup if missing in prod. |
| `ADMIN_PASSWORD` | YES (first run) | Seeds admin user. Min 8 chars. Throws if missing. |
| `DEMO_PASSWORD` | NO | Seeds demo user. Random if missing. |
| `STRIPE_SECRET_KEY` | YES (prod) | Stripe API key |
| `STRIPE_WEBHOOK_SECRET` | YES (prod) | Stripe webhook signature verification |
| `SMTP_PASSWORD` | YES (prod) | Outgoing email |
| `ConnectionStrings__DefaultConnection` | YES (prod) | Database connection |

> Dev defaults are set automatically by `START.bat`. Production deployments MUST set every required var.

### 4.2 appsettings files

| File | Committed? | Contains |
|---|---|---|
| `appsettings.json` | ✅ Yes | Public defaults (empty secrets, schema names, rate limits) |
| `appsettings.Development.json` | ❌ No (gitignored) | LocalDB connection string, dev SMTP, `Email:RequireVerification=false` |
| `appsettings.Production.json` | ❌ No (gitignored) | Production-only overrides |

### 4.3 Frontend env

`GiveAID.Client/.env.development.local.example` is committed; copy to `.env.development.local` (gitignored) and fill in real keys.

---

## 5. Common Tasks

### 5.1 Add a new API endpoint

1. Create DTO in `Application/Features/<Feature>/DTOs/`
2. Create Command/Query + Validator in `Application/Features/<Feature>/Commands|Queries/<Verb>/`
3. Create Handler in same folder. Constructor-inject `IApplicationDbContext` + any service you need.
4. Register controller method in `WebApi/Controllers/<Feature>Controller.cs`
5. Add to API docs: `docs/API_REFERENCE.md`
6. Add Playwright test in `GiveAID.Client/tests/e2e/`

### 5.2 Add a new page (Frontend)

1. Create `src/pages/<PageName>.js` exporting a default React component.
2. Register route in `src/App.js` (or wherever `Routes` is defined).
3. Add link in `src/components/Navbar.js` if user-facing.
4. Use `authService` for API calls. Never raw axios.
5. Test by running `npm start`.

### 5.3 Database migration

```powershell
cd "src/WebApi"
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

Migration files live in `src/Infrastructure/Persistence/Migrations/`. Don't edit manually.

### 5.4 Reset the database

```powershell
# 1. Stop backend
.\STOP.bat

# 2. Delete LocalDB DB
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -Q "DROP DATABASE GiveAIDDB"

# 3. Start backend — auto-creates + seeds
.\START.bat
```

---

## 6. Known Gaps / TODOs (intentional, tracked)

| Gap | Severity | Status |
|---|---|---|
| ~~FluentValidation pipeline not enforced~~ | ~~Medium~~ | ✅ **RESOLVED** — ValidationBehavior registered in MediatR pipeline |
| ~~Soft delete fields unused~~ | ~~Medium~~ | ✅ **RESOLVED** — Global query filter + SaveChanges intercept |
| ~~No audit log~~ | ~~Medium~~ | ✅ **RESOLVED** — Automatic tracking in SaveChangesAsync |
| `ExceptionHandlingMiddleware` overwrites auth exception messages | Medium | Controllers catch `UnauthorizedAccessException` explicitly (see `AuthController.Login`) |
| `EmailOptions` not auto-validated on startup | Low | Manually set `Email:RequireVerification` in dev/prod profiles |
| `appsettings.Development.json` not yet in .gitignore | Low | Add `appsettings.Development.json` to .gitignore before next commit if it contains real secrets |
| No global rate limit on `/auth/*` | Medium | Use ASP.NET Core RateLimiter middleware (configured in `Program.cs`) |

---

## 7. Anti-Patterns to Refuse

If asked to do any of these, push back:

- ❌ Add a hardcoded password/secret anywhere (use env var)
- ❌ Add a new project under `src/` that depends on `WebApi` (WebApi is the entry point, others depend inward)
- ❌ Bypass `MediatR` with direct service calls from controllers
- ❌ Use raw `axios` in frontend instead of the `api` instance
- ❌ Add a `bin/` or `obj/` folder to git
- ❌ Skip writing a test for a new endpoint
- ❌ Commit `appsettings.Development.json` with real secrets
- ❌ Add `npm install <pkg>` without saving to package.json
- ❌ Edit migrations after they're applied

---

## 8. How to Ask the User Questions

The original developer is a domain expert but not a C# / React expert. When unclear:

1. Check if the question has a precedent in this codebase (search first)
2. If multiple valid approaches exist, present 2–3 options with trade-offs (use AskQuestion)
3. Prefer the option that **matches existing patterns** even if a "better" approach exists
4. NEVER assume production deployment context — assume local dev unless told otherwise

---

## 9. Pointers to authoritative docs

| Topic | File |
|---|---|
| Layered architecture + diagrams | `docs/ARCHITECTURE.md` |
| All API endpoints + payloads | `docs/API_REFERENCE.md` |
| Database schema + ERD | `docs/DATABASE.md` |
| Stripe production setup | `docs/STRIPE_PRODUCTION_SETUP.md` |
| Stripe integration test results | `docs/STRIPE_INTEGRATION_TEST_REPORT.md` |
| Critical warnings to AI | `AI_WARNING.md` ← read this too |

---

## 10. Versioning & Changelog

- **2.0.0** (Sept 2026): Clean Architecture rewrite from v1 MVC monolith. Key improvements:
  - Auth refactored: BCrypt password hashing, JWT hardening
  - CQRS via MediatR with ValidationBehavior pipeline
  - Soft delete pattern with global query filters
  - Audit log MVP with automatic change tracking
  - React admin dashboard (25 pages) replacing Razor admin
  - 213 unit/integration/functional tests (100% pass rate)
- Future versions: bump `package.json` (client) and all `.csproj` `Version` (backend) together. Tag in git with `vX.Y.Z` semver.
