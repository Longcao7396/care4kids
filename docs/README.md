# Care4Kids NGO Platform — Master Documentation Index

> **Audience:** This documentation is written for AI coding assistants and human developers
> who need to **quickly understand the project** before making changes. Read this index first,
> then drill into the topic-specific files linked below.

---

## What is this project?

**Care4Kids** (working name "GiveAID") is a full-stack donation / volunteer platform for a
fictional Vietnamese children's welfare NGO. The system supports:

- Public-facing pages (home, causes, campaigns, donations, contact, gallery)
- Authenticated user dashboard (donation history, registrations, profile)
- Admin console (campaigns CRUD, donations list, users, CMS, FAQs, gallery, etc.)
- Two-level cause taxonomy (`parent_cause → sub_cause`)
- Hierarchical campaigns that absorb the legacy `programmes` concept

The application is a hybrid:
- **Backend:** ASP.NET MVC 5 / Web API 2 on .NET Framework 4.7.2, Entity Framework 6
- **Frontend:** React 18 + react-router-dom v6 + react-bootstrap
- **Database:** Microsoft SQL Server (Express)

---

## Table of Contents

| File | Purpose |
|---|---|
| **[ARCHITECTURE.md](./ARCHITECTURE.md)** | High-level system architecture, layers, request lifecycle |
| **[DATABASE.md](./DATABASE.md)** | Schema overview, table list, migrations, ER summary |
| **[CONVENTIONS.md](./CONVENTIONS.md)** | Coding conventions for both backend (C#) and frontend (JS/React) |
| **[PROJECT_MAP.md](./PROJECT_MAP.md)** | Annotated file map: every important file and what it does |
| **[RUNBOOK.md](./RUNBOOK.md)** | How to run, debug, reset, and deploy the project locally |
| **[API_REFERENCE.md](./API_REFERENCE.md)** | REST endpoints summary (auto-derived from controllers) |

---

## Quick Start

If you just opened this project, do this in order:

1. **Read `PROJECT_MAP.md`** — get a mental model of where things live.
2. **Read `ARCHITECTURE.md`** — understand the request flow and authentication model.
3. **Read `RUNBOOK.md`** — boot the project (backend + frontend together via `npm start`).
4. **Read `DATABASE.md`** — only when working on schema, data seeding, or migrations.

---

## Tech Stack at a Glance

| Layer | Technology | Version |
|---|---|---|
| Backend framework | ASP.NET Web API 2 (MVC 5 host) | .NET Framework 4.7.2 |
| ORM | Entity Framework | 6.4.4 |
| Auth | JWT (HS256, custom `[JwtAuthorize]`) | — |
| Password hashing | BCrypt.Net-Next | 4.0.3 |
| Database | Microsoft SQL Server | Express 2019+ |
| Frontend framework | React | 18.2 |
| Routing | react-router-dom | 6.16 |
| UI library | react-bootstrap + bootstrap | 2.9 / 5.3 |
| HTTP client | axios | 1.5 |
| Build tool | react-scripts | 5.0.1 |
| Process orchestration | concurrently | 10.0.5 |

---

## Demo Accounts (development only)

```
Admin   →  admin@give-aid.org   /  Admin@123     (role: SuperAdmin)
User    →  user@example.com     /  User@123      (role: User)
```

If passwords stop working, call `POST /api/auth/bootstrap` with the SuperAdmin JWT to reset.

---

## Project Layout (top-level)

```
project NGO/
├── GiveAID.Client/         ← React frontend (npm start runs both)
├── GiveAID.Web/            ← ASP.NET backend (IIS Express)
├── GiveAID.Web.sln         ← Visual Studio solution
├── docs/                   ← This documentation folder
├── *.sql                   ← Database migration scripts (apply in order)
└── README.md               ← (you are here's neighbour)
```

---

## When in doubt…

| Question | Look in |
|---|---|
| "Where is the route for X?" | `GiveAID.Client/src/App.js` (public + admin) |
| "Where is the API for X?" | `GiveAID.Web/Controllers/*Controller.cs` |
| "What's the DB schema for X?" | `DATABASE.md` + the matching `EntityModels.cs` class |
| "How do I run it?" | `RUNBOOK.md` |
| "How is auth implemented?" | `ARCHITECTURE.md` → "Authentication" |
| "What's the styling system?" | `CONVENTIONS.md` → "Frontend Style" |

---

_Last updated: 2026-09-15_
