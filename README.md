# GiveAID v2.0

> NGO donation & welfare platform — Clean Architecture rewrite
> Public React site + Admin Razor console + ASP.NET Core WebApi

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Tests](https://img.shields.io/badge/tests-169%2F169-success)]()
[![License](https://img.shields.io/badge/license-Proprietary-blue)]()

## Database Setup — single source of truth

This project targets **one** local SQL Server instance for day-to-day development:

```
(localdb)\MSSQLLocalDB
```

This is the instance baked into `src/WebApi/appsettings.Development.json` and the
fallback in `src/Infrastructure/Persistence/GiveAIDDbContextFactory.cs`. It
ships with **Visual Studio** (and is installed by the **.NET SDK** on Windows),
so no separate SQL Server install is required.

> If you previously used `.\SQLEXPRESS` you were almost certainly looking at the
> **wrong database** when you saw empty tables after a migration or upload. All
> PowerShell scripts and SQL files now default to LocalDB. See
> [Switching to SQL Server Express](#switching-to-sql-server-express-optional)
> below if you really need Express.

### Verify the database is reachable

A single PowerShell script is the canonical health check. It detects which
instance your appsettings actually point at, pings it, checks that
`GiveAIDDB` exists, and counts rows in the key tables.

```powershell
powershell -ExecutionPolicy Bypass -File verify-database.ps1
```

Expected: green `[OK]` lines for **Connectivity**, **Database existence**, and
**Row counts**, followed by a `[OK] Database is reachable…` verdict. Exit
code `0` means everything is healthy; non-zero means the script will print
hints specific to the failure.

### Set up the database from scratch

Option A — **let the application do it** (recommended for new devs):

```powershell
dotnet run --project src/WebApi/GiveAID.V2.WebApi.csproj
# First start runs migrations + seeds admin user automatically.
```

Option B — **explicit apply** (preferred for CI):

```powershell
# From repo root
powershell -ExecutionPolicy Bypass -File database\99_Apply-All.ps1
# Reads (localdb)\MSSQLLocalDB from appsettings.Development.json.
```

Option C — **sqlcmd** (debugging only):

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -Q "IF DB_ID('GiveAIDDB') IS NULL CREATE DATABASE GiveAIDDB"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -d master -i "database\01_CreateDatabase_V2.sql"
```

### Switching to SQL Server Express (optional)

If you have SQL Server Express installed and want to use it instead:

1. Enable **TCP/IP** and **Named Pipes** in
   `SQL Server Configuration Manager → Protocols for SQLEXPRESS`.
2. Update the connection string in
   `src/WebApi/appsettings.Development.json` to
   `Server=.\SQLEXPRESS;Database=GiveAIDDB;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True;Connect Timeout=15`.
3. Tell every script/seed to use Express instead of LocalDB:
   ```powershell
   powershell -File verify-database.ps1 -Server ".\SQLEXPRESS"
   powershell -File database\99_Apply-All.ps1 -Server ".\SQLEXPRESS"
   ```

### Troubleshooting — "I see empty tables!"

Run the verification script first. The most common causes are:

| Symptom | Likely cause | Fix |
|---|---|---|
| `Could not connect to (localdb)\MSSQLLocalDB` | LocalDB not installed or stopped | `sqllocaldb start MSSQLLocalDB` |
| `Database 'GiveAIDDB' does NOT exist` | First run never happened | `dotnet run --project src/WebApi` (auto-creates) **or** `database\99_Apply-All.ps1` |
| `MISSING (table does not exist)` | Schema out of sync | `database\99_Apply-All.ps1` (drops + recreates) |
| `Some key tables are empty` | Seed never ran | `database\99_Apply-All.ps1` (idempotent re-seed) |
| Empty tables in **SSMS** but app shows data | You connected to a different instance | Re-check: `SELECT @@SERVERNAME` in the same SSMS query window |

## Overview

GiveAID v2.0 is a full rewrite of the legacy ASP.NET WebForms + IIS Express stack on a
modern **Clean Architecture** foundation. The solution separates concerns into five layers,
supports CQRS via MediatR, and ships with a comprehensive automated test suite (169 tests,
100% pass rate).

## Tech Stack

| Layer            | Technology                                                 |
|------------------|------------------------------------------------------------|
| Domain           | C# / .NET 8, pure POCOs, no dependencies                   |
| Application      | MediatR (CQRS), FluentValidation, AutoMapper               |
| Infrastructure   | EF Core 8 (SQL Server), JWT, SMTP, Stripe, MemoryCache     |
| WebApi           | ASP.NET Core 8 Web API, JWT bearer, Scalar OpenAPI         |
| Web (Admin)      | ASP.NET Core 8 MVC + Razor Pages, AdminLTE 3.2             |
| Client (Public)  | React 18 + React Router + Axios                            |
| Tests            | xUnit + FluentAssertions + Moq + WebApplicationFactory     |

## Repository Layout

```
project-NGO/
├── src/
│   ├── Domain/                    # Entities, value objects, enums (no deps)
│   ├── Application/               # CQRS handlers, validators, DTOs
│   ├── Infrastructure/            # EF Core, JWT, Email, Payment, Cache
│   ├── WebApi/                    # REST API (port 5231)
│   └── Web/                       # Admin console (port 5069)
├── tests/
│   ├── Domain.UnitTests/          # 71 tests
│   ├── Application.UnitTests/     # 43 tests
│   ├── Infrastructure.IntegrationTests/  # 28 tests
│   └── WebApi.FunctionalTests/    # 27 tests
├── GiveAID.Client/                # React 18 public site (port 3000)
├── database/                      # SQL migrations + seeds
├── docs/                          # Architecture, API, deployment
└── GiveAID.V2.slnx
```

## Quick Start

### Prerequisites

- .NET 8 SDK
- Node.js 18+
- Visual Studio 2022 / Rider / VS Code (ships with **LocalDB** — the project's
  default SQL Server instance). No separate SQL Server install needed.

### 1. Database

```powershell
# Recommended: let EF Core create + seed it on first run
dotnet run --project src/WebApi/GiveAID.V2.WebApi.csproj

# Or apply the full schema + seeds explicitly
powershell -ExecutionPolicy Bypass -File database\99_Apply-All.ps1

# Or just smoke-test the connection (no writes)
powershell -ExecutionPolicy Bypass -File verify-database.ps1
```

See the **Database Setup** section above for the canonical instance
(`(localdb)\MSSQLLocalDB`) and how to switch to `.\SQLEXPRESS` if you
already have it installed.

### 2. Backend

```powershell
cd "C:\Users\admin\Desktop\project NGO.v2"
dotnet restore
dotnet build
dotnet run --project src/WebApi/GiveAID.V2.WebApi.csproj
# API at http://localhost:5231
# OpenAPI at http://localhost:5231/scalar/v1
```

### 3. Admin Console

```powershell
dotnet run --project src/Web/GiveAID.V2.Web.csproj
# Admin at http://localhost:5069
# Login: admin / Admin@123  (or admin@give-aid.org)
```

### 4. Public Site

```powershell
cd GiveAID.Client
npm install
npm start
# Site at http://localhost:3000
```

### 5. Run Tests

```powershell
dotnet test GiveAID.V2.slnx
# Expected: 169 passed, 0 failed
```

## Default Credentials

| Role       | Email                  | Password    |
|------------|------------------------|-------------|
| Admin      | admin@give-aid.org     | Admin@123   |
| User       | user@give-aid.org      | User@123    |

## Architecture Highlights

- **Clean Architecture** with strict dependency direction (Domain ← Application ← Infrastructure ← WebApi/Web)
- **CQRS** via MediatR — Commands and Queries segregated in `src/Application/Features/`
- **Envelope JSON contract** — every response is `{success, message, data, errors?}` for predictable client handling
- **JWT auth** with refresh tokens, role-based policy (`Admin`)
- **Rate limiting** (100 req/min/IP) via `AspNetCoreRateLimit`
- **AdminLTE 3.2** Razor console for back-office staff
- **React 18** SPA with axios interceptors that auto-attach JWT and unwrap envelopes

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for the full architecture document.

## API Surface

24 controllers covering authentication, causes, campaigns, donations, gallery,
team/achievements/careers, FAQs, contact, conversations, invitations, CMS, statistics, and admin tools.

All endpoints live under `/api/v1/`. See [`docs/API_REFERENCE.md`](docs/API_REFERENCE.md).

## Project Status

| Phase | Description                              | Status |
|-------|------------------------------------------|--------|
| 1     | Planning, scaffolding, dependencies      | ✅      |
| 2     | Domain entities + enums                  | ✅      |
| 3     | Application (CQRS + validators)          | ✅      |
| 4     | Infrastructure (EF, JWT, Email, Stripe)  | ✅      |
| 5     | WebApi (24 controllers + Scalar)         | ✅      |
| 6     | Admin console (Razor + AdminLTE)         | ✅      |
| 7     | Tests (169 tests, 100% pass)             | ✅      |
| 8     | React client refactor                    | ✅      |
| 9     | Documentation + cutover                  | ✅      |

## Documentation

- [Architecture](docs/ARCHITECTURE.md)
- [API Reference](docs/API_REFERENCE.md)
- [Database Schema](docs/DATABASE.md)
- [Project Map](docs/PROJECT_MAP.md)
- [Conventions](docs/CONVENTIONS.md)
- [Migration Guide](docs/MIGRATION_GUIDE.md)
- [Testing Strategy](docs/TESTING.md)
- [Deployment & Runbook](docs/DEPLOYMENT.md)
- [Production Build Guide](docs/PRODUCTION_BUILD.md)

## 🤖 For AI Agents

**If you're an AI agent reading this codebase, START with [AI_GUIDE.md](AI_GUIDE.md)**

It contains quick patterns for:
- JWT authentication flow
- CQRS/MediatR patterns
- Clean Architecture structure
- Troubleshooting shortcuts

---

## License

Proprietary — internal NGO project. All rights reserved.
