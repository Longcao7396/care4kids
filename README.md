# GiveAID v2.0

> NGO donation & welfare platform — Clean Architecture rewrite
> Public React site + Admin Razor console + ASP.NET Core WebApi

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)]()
[![Tests](https://img.shields.io/badge/tests-169%2F169-success)]()
[![License](https://img.shields.io/badge/license-Proprietary-blue)]()

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
- SQL Server 2019+ (Express works)
- Visual Studio 2022 / Rider / VS Code

### 1. Database

```powershell
# Create database
sqlcmd -S .\SQLEXPRESS -Q "CREATE DATABASE GiveAIDDB"

# Run migrations (in order)
cd database/migrations
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i 001_InitialSchema.sql
sqlcmd -S .\SQLEXPRESS -d GiveAIDDB -i 002_SeedData.sql
```

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
