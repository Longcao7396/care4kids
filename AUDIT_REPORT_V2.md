# GiveAID v2.0 — Comprehensive QA Audit Report
**Date:** 2026-09-19
**Auditor:** Senior QA + Rebuild Strategist
**Scope:** Backend API (ASP.NET MVC 5 / Web API 2, .NET 4.7.2) + React 18 Frontend + Admin Console

---

## Executive Summary

| Component | Status | Details |
|-----------|--------|---------|
| Backend Build | ✅ PASS | 0 errors, 0 warnings |
| React Build | ✅ PASS | Compiled successfully (CRA build) |
| Unit/Integration Tests | ⚠️ PARTIAL | 121 passed, 66 failed, 32 skipped (DB connectivity) |
| Backend Running | ✅ PORT 5231 | Confirmed `LISTENING` + `ESTABLISHED` |
| Frontend Running | ✅ PORT 3000 | Confirmed `LISTENING` |
| Public API Endpoints | 18/20 PASS | 2 missing/wrong-path routes |
| Authenticated API Endpoints | 🔴 BLOCKED | Role case mismatch — every admin route returns 403 |
| Admin Console | 🔴 BLOCKED | Cannot access any admin route |
| Database | ⚠️ PARTIAL | 22 tables; 4 tables completely unseeded |

**Total Features Tested: ~45 | Passed: ~18 | Failed/Blocked: ~27**

---

## BLOCKER Bugs (Must Fix Before Any Other Work)

### BLK-001: JWT Role Claim Case Mismatch → Admin Console 100% Broken
| Field | Value |
|-------|-------|
| **Surface** | WebApi (backend) |
| **Symptom** | Every `[JwtAuthorize(Roles = "SuperAdmin,Admin")]` endpoint returns 403 Forbidden. Admin dashboard, donations, users, campaigns, email logs — ALL inaccessible. |
| **Root Cause** | `JwtHelper.GenerateToken()` at `JwtHelper.cs:42` creates role claim as `ClaimTypes.Role` with value `user.Role` ("Admin" from DB). The running JWT decodes to `"role": "admin"` (lowercase). `JwtAuthorizeAttribute.cs:38` does a case-sensitive `allowed.Contains(userRole)` against "SuperAdmin,Admin" (PascalCase). Returns `false` for "admin". |
| **DB Check** | `Users.role = 'Admin'` (PascalCase) |
| **JWT Decode** | `"role": "admin"` (lowercase in token) |
| **CheckAdmin()** | `JwtHelper.CheckAdmin()` at `JwtHelper.cs:169` does `role == "SuperAdmin" || role == "Admin"` (PascalCase). Returns `false` for "admin". |
| **File:Line** | `GiveAID.Web\Helpers\JwtAuthorizeAttribute.cs:38` |
| **Fix** | Add `StringComparer.OrdinalIgnoreCase` to the role check: `allowed.Contains(userRole, StringComparer.OrdinalIgnoreCase)` |
| **Impact** | **All 19 admin-gated endpoints permanently return 403. No admin can access their own console.** |

### BLK-002: LoginRequest Source/DLL Mismatch — Frontend Sends `username` but Source Has `Email` Only
| Field | Value |
|-------|-------|
| **Surface** | WebApi |
| **Symptom** | Running server accepts `{username: "admin", password: "..."}` → 200 OK with correct user data. Source code `LoginRequest` only has `Email` field. Source and compiled DLL are out of sync. |
| **Root Cause** | Source code `AuthController.cs:299-303` has `LoginRequest { Email, Password }`. Running DLL must have `Username` field added + dual query. `LoginPage.js` comment says "email is no longer accepted as a login identifier" — this fix was applied to the running server but NOT committed to source. |
| **File:Line** | `GiveAID.Web\Controllers\AuthController.cs:299-303` |
| **Fix** | Ensure source matches compiled DLL: add `Username` to `LoginRequest`, query by both username and email. Commit fix to source control. |
| **Impact** | Source/DLL divergence — next deployment may silently break login. |

---

## MAJOR Bugs

### MAJ-001: Empty Data — Achievements, Team, Careers, Donations (4 Tables Unseeded)
| Field | Value |
|-------|-------|
| **Surface** | WebApi + React |
| **Pages Affected** | `/about/achievements`, `/about/team`, `/about/careers`, `/my-donations` |
| **Symptom** | All 4 endpoints return empty arrays. DB confirms: `Achievements`=0, `team_members`=0, `Careers`=0, `Donations`=0. |
| **Root Cause** | No seed data for these 4 tables. |
| **Fix** | Write and run SQL seed scripts for each table (see REBUILD_PLAN.md Phase B). |
| **Severity** | Major — 4 pages are functionally empty. |

### MAJ-002: EF `[Table]` Mismatch — `cms_pages` vs `CmsPages`
| Field | Value |
|-------|-------|
| **Surface** | WebApi |
| **Symptom** | `CmsPagesController` uses `db.CmsPages` → maps to `CmsPages` table. DB has `cms_pages` (snake_case from migration scripts). All CMS admin operations silently fail. |
| **Root Cause** | SQL migrations created `cms_pages`. EF model `[Table("CmsPages")]` expects `CmsPages`. Context initializer is `null` — no auto-mapping. |
| **File:Line** | `EntityModels.cs:540` — `[Table("CmsPages")]` |
| **Fix** | Change `[Table("CmsPages")]` to `[Table("cms_pages")]` |

### MAJ-003: EF `[Table]` Mismatch — `team_members` vs `TeamMembers`
| Field | Value |
|-------|-------|
| **Surface** | WebApi |
| **Symptom** | `TeamMembers` DbSet queries `team_members` via EF → fails (table name mismatch). Team admin editing is broken. |
| **Root Cause** | Same as MAJ-002. |
| **File:Line** | `EntityModels.cs:795` — `[Table("TeamMembers")]` |
| **Fix** | Change `[Table("TeamMembers")]` to `[Table("team_members")]` |

### MAJ-004: 404 Route Mismatches — config.js References Non-existent Endpoints
| Field | Value |
|-------|-------|
| **Surface** | WebApi + React |
| **Symptom** | `config.js` defines 4 endpoint patterns that don't exist. React components calling these get silent failures. |
| **404** | `GET /api/v1/achievements/stats` — `config.js:71` → `AchievementsController` has no `/stats` route |
| **404** | `GET /api/v1/causes/stats` — `config.js:34` → `CausesController` has no `/stats` route |
| **404** | `GET /api/v1/gallery/programmes` — `config.js:152` → `GalleryController` has no `/programmes` route |
| **404** | `GET /api/v1/statistics/dashboard` — `config.js:179` → `StatisticsController` uses `Route("overview")` not `"dashboard"` |
| **Fix** | Either add missing routes to controllers (Option A, adds features) OR update `config.js` to match actual routes (Option B, quick cleanup). |
| **Severity** | Major — silent API failures in multiple React components. |

### MAJ-005: ContactsController Requires Auth for Public Form Submission
| Field | Value |
|-------|-------|
| **Surface** | WebApi |
| **Symptom** | `POST /api/v1/contacts` (public contact form) returns 401. Anonymous users cannot submit contact messages. |
| **Root Cause** | `ContactsController.cs:10` — class-level `[JwtAuthorize(Roles = "SuperAdmin,Admin")]` blocks all endpoints including the public form submission. |
| **File:Line** | `ContactsController.cs:10` |
| **Fix** | Add `[AllowAnonymous]` to the `POST /contacts` endpoint. |

### MAJ-006: Admin Routes Use Wrong Prefix — React Calls `/admin/*` but Controllers Use `/api/*`
| Field | Value |
|-------|-------|
| **Surface** | WebApi |
| **Symptom** | React `AdminDonationsPage.js` calls `api.get('/admin/donations')` → 404. `DonationsController` is at `api/donations`. |
| **Root Cause** | Admin React pages use `/admin/*` paths but no admin-prefixed controllers exist. Only `AdminDashboardController` uses `[RoutePrefix("api/admin")]`. |
| **Files** | `AdminDonationsPage.js`, `AdminUsersPage.js`, `AdminCampaignPage.js` all use `/admin/*` prefix |
| **Fix** | Either create admin-prefixed controllers OR update React admin pages to use correct controller routes. |
| **Severity** | Major — all admin data-fetching calls return 404 in addition to BLK-001's 403. |

---

## MINOR Bugs

### MIN-001: `auth/me` Returns Incomplete User Data
| Field | Value |
|-------|-------|
| **Surface** | WebApi |
| **Symptom** | `GET /api/v1/auth/me` returns only `{userId, email}`. Missing: `role`, `username`, `fullName`. |
| **File:Line** | `AuthController.cs:100-110` |
| **Fix** | Add missing fields to the response. |

### MIN-002: Test Suite — 66 Failures Due to DB Connectivity
| Field | Value |
|-------|-------|
| **Surface** | Tests |
| **Symptom** | `dotnet test`: 66 failed (all DB connectivity), 121 passed, 32 skipped. |
| **Root Cause** | `TestDbContextFactory.cs:111` — `ExecuteDelete` fails because test runner can't connect to SQL Server LocalDB. |
| **Severity** | Minor — not a code issue, environment configuration problem. |

### MIN-003: Stripe Test Keys Not Configured
| Field | Value |
|-------|-------|
| **Surface** | React + WebApi |
| **Symptom** | `DonatePage.js` imports `loadStripe` but no Stripe publishable key is set. Donation UI renders but no real payment processing. |
| **Severity** | Minor — donation flow cannot be tested end-to-end. |

---

## Database Status

| Table | DB Rows | Issue |
|-------|---------|-------|
| `Achievements` | **0** | 🔴 Unseeded |
| `team_members` | **0** | 🔴 Unseeded + EF mismatch |
| `Careers` | **0** | 🔴 Unseeded |
| `Donations` | **0** | 🔴 Unseeded |
| `Campaigns` | 8 | ✅ OK |
| `Causes` | 21 | ✅ OK |
| `Gallery` | 16 | ✅ OK |
| `FAQs` | 6 | ✅ OK |
| `Organizations` | ? | ✅ OK (Supporters) |
| `CampaignReports` | ? | ✅ OK |
| `cms_pages` | ? | ⚠️ EF mismatch |
| `Invitations` | ? | ✅ OK |
| `Conversations` | ? | ✅ OK |
| `Users` | 2 | ✅ OK |
| `EmailLogs` | ? | ✅ OK |

---

## Test Suite Results

```
dotnet test — GiveAID.Tests.dll
  Failed:     66  (all DB connectivity — not assertion failures)
  Passed:    121
  Skipped:    32
  Total:     219
  Duration:   1m 41s
```

All 66 failures are `TestDbContextFactory.ExecuteDelete` failures — SQL Server LocalDB is not accessible from the test runner process. This is an **environment configuration issue**, not a code defect.

---

## React Public Site — Expected Findings (API-Based Prediction)

| Page | Status | Reason |
|------|--------|--------|
| HomePage | ✅ Should load | campaigns/causes data present |
| CampaignsPage | ✅ Should load | 8 campaigns in DB |
| CampaignDetailPage | ⚠️ PARTIAL | Loads but 0 donations shown |
| AboutPage | ⚠️ PARTIAL | Empty sections (Achievements, Team unseeded) |
| OurTeamPage | ❌ EMPTY | `team_members` = 0 rows |
| AchievementsPage | ❌ EMPTY | `Achievements` = 0 rows |
| CareersPage | ❌ EMPTY | `Careers` = 0 rows |
| SupportersPage | ✅ Should load | Organizations data present |
| GalleryPage | ✅ Should load | 16 gallery images |
| HelpCentrePage | ✅ Should load | 6 FAQs |
| ContactPage | ⚠️ BROKEN | POST requires auth |
| CausesPage | ✅ Should load | 21 causes |
| LoginPage | ✅ Works | Login succeeds (username accepted) |
| RegisterPage | ✅ Should work | No auth required |
| PrivacyPage | ✅ Should load | Static CMS page |
| TermsPage | ✅ Should load | Static CMS page |
| Admin Console | ❌ BLOCKED | BLK-001 (403) + MAJ-006 (404) |

---

## Complete Failures Table

| ID | Surface | Page/Endpoint | Symptom | Root Cause | Severity |
|----|---------|--------------|---------|------------|----------|
| BLK-001 | WebApi | All `[JwtAuthorize(Roles="SuperAdmin,Admin")]` endpoints | 403 Forbidden — every admin endpoint blocked | Role case mismatch: JWT `"admin"` vs attribute `"Admin"` — `JwtAuthorizeAttribute.cs:38` | Blocker |
| BLK-002 | WebApi | `POST /api/v1/auth/login` | Running DLL accepts `{username}` but source only has `Email` field | Source/DLL divergence — compiled DLL has `Username` field not in source | Blocker |
| MAJ-001 | DB | `Achievements` | 0 rows | No seed data | Major |
| MAJ-002 | DB | `team_members` | 0 rows | No seed data + EF `[Table]` mismatch | Major |
| MAJ-003 | DB | `Careers` | 0 rows | No seed data | Major |
| MAJ-004 | DB | `Donations` | 0 rows | No seed data | Major |
| MAJ-005 | WebApi | EF `CmsPages` DbSet | CMS admin ops silently fail | `[Table("CmsPages")]` vs DB `cms_pages` — `EntityModels.cs:540` | Major |
| MAJ-006 | WebApi | EF `TeamMembers` DbSet | Team admin ops silently fail | `[Table("TeamMembers")]` vs DB `team_members` — `EntityModels.cs:795` | Major |
| MAJ-007 | WebApi | `/api/v1/achievements/stats` | 404 | No `/stats` route in `AchievementsController` | Major |
| MAJ-008 | WebApi | `/api/v1/causes/stats` | 404 | No `/stats` route in `CausesController` | Major |
| MAJ-009 | WebApi | `/api/v1/gallery/programmes` | 404 | No `/programmes` route in `GalleryController` | Major |
| MAJ-010 | WebApi | `/api/v1/statistics/dashboard` | 404 | `StatisticsController` uses `Route("overview")` — `StatisticsController.cs:32` | Major |
| MAJ-011 | WebApi | `POST /api/v1/contacts` | 401 for anonymous users | Class-level `[JwtAuthorize]` blocks public form — `ContactsController.cs:10` | Major |
| MAJ-012 | WebApi + React | React admin pages use `/admin/*` routes | 404 — no matching controllers | Admin React calls `api/admin/donations` but `DonationsController` is at `api/donations` | Major |
| MIN-001 | WebApi | `GET /api/v1/auth/me` | Returns only `{userId, email}` | Incomplete response — `AuthController.cs:100-110` | Minor |
| MIN-002 | Tests | All 66 failing tests | DB connectivity failures | Test runner can't reach SQL Server LocalDB | Minor |
| MIN-003 | React | `/donate` | Stripe not configured | No `REACT_APP_STRIPE_KEY` env var | Minor |

---

## Recommendations (Ordered by ROI)

1. **Fix BLK-001** — 1-line fix in `JwtAuthorizeAttribute.cs` → Unblocks all 19 admin endpoints
2. **Fix BLK-002** — Add `Username` to `LoginRequest`, commit to source → Fixes source/DLL divergence
3. **Fix MAJ-005 + MAJ-006** — Update 2 `[Table]` attributes → Unblocks CMS and Team admin
4. **Seed 4 tables** — Write and run seed SQL → Fills empty pages
5. **Fix MAJ-004** — Align `config.js` with actual routes → Prevents silent failures
6. **Fix MAJ-011** — Add `[AllowAnonymous]` to `POST /contacts` → Enables contact form
7. **Fix MAJ-012** — Standardize admin API routes → All admin pages can fetch data
8. **Configure Stripe** — Set test keys → Enables donation end-to-end testing
