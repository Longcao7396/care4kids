# GiveAID v2.0 — Documentation

Welcome to the GiveAID v2.0 documentation set. These documents are the authoritative
reference for the architecture, API surface, deployment procedures, and developer workflow.

## Table of Contents

| Document | Description |
|----------|-------------|
| [Architecture](ARCHITECTURE.md) | Clean Architecture layers, dependency rules, CQRS, request lifecycle |
| [API Reference](API_REFERENCE.md) | All REST endpoints with request/response schemas, auth requirements |
| [Database Schema](DATABASE.md) | ER diagram, tables, indexes, migration order |
| [Project Map](PROJECT_MAP.md) | Repository layout, file responsibilities, naming conventions |
| [Conventions](CONVENTIONS.md) | C# coding standards, Git workflow, commit message format |
| [Migration Guide](MIGRATION_GUIDE.md) | v1 (legacy ASP.NET WebForms) → v2.0 migration steps |
| [Testing Strategy](TESTING.md) | Test pyramid, xUnit patterns, fixtures, coverage targets |
| [Deployment & Runbook](DEPLOYMENT.md) | Local + production deployment, ops procedures, troubleshooting |

## Quick Links

- [Project README](../README.md)
- [API OpenAPI spec](../src/WebApi) (Scalar UI at runtime)
- [Database migrations](../database/migrations/)

## How to Use These Docs

1. **New developer onboarding** — read [Architecture](ARCHITECTURE.md) first, then [Project Map](PROJECT_MAP.md), then [Conventions](CONVENTIONS.md).
2. **Integrating with the API** — start with [API Reference](API_REFERENCE.md). Live docs available at `http://localhost:5231/scalar/v1`.
3. **Database changes** — see [Database Schema](DATABASE.md) for migration order and ER diagram.
4. **Going to production** — read [Deployment & Runbook](DEPLOYMENT.md) end-to-end.
5. **Migrating from v1** — follow [Migration Guide](MIGRATION_GUIDE.md) step by step.

## Document Status

All documents are kept in sync with the codebase as part of the cutover phase (Phase 9).
Last reviewed: September 2026.
