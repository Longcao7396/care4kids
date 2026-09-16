# Architecture

> Audience: AI coding assistants. **Read this before touching any file.**
> Goal: build a complete mental model of the system so you don't break invariants.

---

## 1. High-level topology

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              Browser (React 18)                          │
│                                                                          │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────────────┐ │
│  │ Public     │  │ Auth       │  │ User       │  │ Admin              │ │
│  │ Pages      │  │ Pages      │  │ Dashboard  │  │ Console            │ │
│  │ (Home,     │  │ (Login,    │  │ (MyDon.,   │  │ (/admin/*,         │ │
│  │ Campaigns, │  │ Register)  │  │ Reg.,      │  │  AdminPageFrame    │ │
│  │ Causes)    │  │            │  │ Profile)   │  │  wrapper)          │ │
│  └─────┬──────┘  └─────┬──────┘  └─────┬──────┘  └─────────┬──────────┘ │
│        └──────────────┴──────┬─────────┴───────────────────┘            │
│                              │                                            │
│                     axios instance (with JWT interceptor)                │
│                              │                                            │
│                    localStorage: GiveAID_token                            │
└──────────────────────────────┼───────────────────────────────────────────┘
                               │  HTTP (JSON)
                               ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                  ASP.NET Web API 2 (IIS Express)                          │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │ Global.asax.cs                                                    │   │
│  │   • Fail-fast check on JWT config                                 │   │
│  │   • SeedDatabase() call (admin + demo users)                       │   │
│  │   • CORS preflight handler                                        │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │ Controllers/                                                      │   │
│  │   AuthController, CampaignsController, CausesController,           │   │
│  │   DonationsController, UsersController, AdminDashboardController, │   │
│  │   AchievementsController, CareersController, …                     │   │
│  │                                                                   │   │
│  │   Attributes per route:                                            │   │
│  │     [JwtAuthorize]                  → any authenticated user      │   │
│  │     [JwtAuthorize(Roles="…")]       → role-gated                  │   │
│  │     (no attribute)                  → public endpoint             │   │
│  └─────────────────────────┬────────────────────────────────────────┘   │
│                            │                                              │
│  ┌─────────────────────────▼────────────────────────────────────────┐   │
│  │ Helpers/                                                          │   │
│  │   JwtHelper            — token validation, role check             │   │
│  │   JwtAuthorizeAttribute — declarative [JwtAuthorize] filter       │   │
│  │   JwtSettings          — typed config accessor (throws on bad)    │   │
│  │   PasswordHasher       — BCrypt wrap (workFactor 11)              │   │
│  │   AuthBootstrap        — one-shot admin/demo hash utility         │   │
│  │   EntityExtensions     — UpdatedAtSafe() extension method          │   │
│  └─────────────────────────┬────────────────────────────────────────┘   │
│                            │                                              │
│  ┌─────────────────────────▼────────────────────────────────────────┐   │
│  │ Data/GiveAIDContext.cs (EF 6 DbContext)                            │   │
│  │   • DbSets for all entities                                        │   │
│  │   • snake_case column convention                                   │   │
│  │   • Precision rules for money columns                              │   │
│  └─────────────────────────┬────────────────────────────────────────┘   │
└────────────────────────────┼───────────────────────────────────────────┘
                               │  ADO.NET / EF6
                               ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                  Microsoft SQL Server  (GiveAIDDB)                        │
│   See DATABASE.md for schema details and migration order.                │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Request lifecycle (typical)

1. **Browser** issues an axios call (e.g. `GET /api/campaigns/featured`).
2. **api.js interceptor** attaches `Authorization: Bearer <jwt>` from `localStorage`.
3. **Web API routing** dispatches to the matching `[RoutePrefix]api/...` controller.
4. **`[JwtAuthorize]` filter** (if present) validates the JWT via `JwtHelper.ValidateToken`.
5. **Controller action** runs LINQ queries against `GiveAIDContext` (EF 6).
6. **EF6** translates to SQL, returns entities.
7. **Controller** maps entities to DTOs (`CampaignDto`, `ViewModels.cs`) and returns `IHttpActionResult`.
8. **axios** receives JSON; UI updates state.

---

## 3. Authentication flow

```
┌────────────┐                ┌──────────────────┐                  ┌─────────┐
│  LoginPage │ POST /login    │  AuthController  │  verify BCrypt  │  Users  │
│  (React)   ├───────────────►│  (no [auth])     ├─────────────────►│  table  │
└────────────┘                │                  │  User@123 → hash │         │
                              │  if match:       │                  └─────────┘
                              │  generate JWT    │
                              │  (HS256)         │
                              └────────┬─────────┘
                                       │  { token, user }
                                       ▼
                              ┌──────────────────┐
                              │  localStorage    │
                              │  GiveAID_token   │
                              └──────────────────┘
```

**JWT validation rules** (see `JwtHelper.ValidateToken`):

- Validates signing key against `JwtSettings.Secret` (from `Web.config`)
- Validates issuer / audience
- Validates lifetime (`ClockSkew = TimeSpan.Zero`)
- **Re-validates the user is still active in DB** (so deactivating a user invalidates their token immediately)

**Role hierarchy:**

| Role | Powers |
|---|---|
| `User` | Donate, register for events, edit own profile |
| `Admin` | All User actions + admin console (read/manage most entities) |
| `SuperAdmin` | All Admin actions + delete-only operations + `POST /api/auth/bootstrap` |

**Why no OWIN bearer middleware?** The project doesn't wire up `app.UseOAuthBearerTokens(...)`.
Instead, we use a **custom `[JwtAuthorize]`** attribute that manually calls `JwtHelper.ValidateToken`.
This keeps the dependency surface small and the auth path debuggable. **Do not** add OWIN bearer
middleware without first auditing every controller — `[Authorize]` (without our wrapper) will
return 401 on every call and break the API.

---

## 4. The "Programme → Campaign" merge

The original schema had both `Campaigns` and `Programmes` tables. The `Programmes` table was for
events (registration_required, max_participants, etc.) while `Campaigns` was for donations.
We've **merged Programmes into Campaigns**:

- `Campaign` entity now carries `ProgrammeType`, `RegistrationRequired`, `MaxParticipants`,
  `ExpectedBudget`, `ActualBudget`.
- The legacy `Programmes` table still exists (kept for Gallery FK + read-only access via the
  obsolete `ProgrammesController`).
- Frontend pages `ProgrammesPage` and `ProgrammeDetailPage` are deprecated wrappers that
  redirect to `/campaigns?eventsOnly=true` and `/campaigns/:id`.

**Migration:** `NGO_Database_CampaignProgramme_Merge.sql` (run after causes restructure).

If you need to add a new event-style field, **add it to the Campaign entity**, not to Programme.

---

## 5. CORS

`WebApiConfig.cs` registers `EnableCorsAttribute` with these dev origins:

```
http://localhost:3000
http://localhost:3001
http://127.0.0.1:3000
```

The browser sends `withCredentials = true` (for the JWT in headers), so the CORS attribute
**must** set `SupportsCredentials = true`. `Global.asax.Application_BeginRequest` also handles
OPTIONS preflight manually for the same origins as a belt-and-suspenders.

For production: add the production origin to `Cors:AllowedOrigins` in `Web.config` and to the
`EnableCorsAttribute` origins list.

---

## 6. Frontend layers

```
┌─────────────────────────────────────────────────────────────┐
│  pages/             ← Route components (top-level screens)   │
├─────────────────────────────────────────────────────────────┤
│  components/        ← Reusable UI (Navbar, Footer, etc.)     │
├─────────────────────────────────────────────────────────────┤
│  contexts/          ← React contexts (AuthContext)           │
├─────────────────────────────────────────────────────────────┤
│  services/          ← API access layer (index.js + api.js)   │
│                      index.js   — one method per endpoint    │
│                      api.js     — axios instance + JWT int.  │
├─────────────────────────────────────────────────────────────┤
│  data/              ← Static fallback samples (SAMPLE_…)     │
├─────────────────────────────────────────────────────────────┤
│  styles/            ← Global CSS (Care4Kids design tokens)   │
└─────────────────────────────────────────────────────────────┘
```

**Admin pages** all wrap their content in `<AdminPageFrame>` (a shared layout component) which
provides the page header, error/success banners, and a consistent look. When adding a new admin
page, **always use AdminPageFrame** — don't roll your own header.

---

## 7. Error handling contract

Every API response (success or failure) uses the shape:

```json
{ "success": true|false, "message": "…", "data": {…} }
```

or for validation errors:

```json
{ "success": false, "message": "Validation failed", "errors": { "fieldName": ["…"] } }
```

Frontend services unwrap `response.data` (axios) and return the inner `data` object. When
adding a new endpoint, **always** return one of these shapes — never return raw entities or
anonymous objects.

---

## 8. Invariants (do not break)

1. **DB schema is canonical.** EF is configured with `Database.SetInitializer(null)` so EF
   never tries to create/modify the schema. Schema changes must come from the `*.sql`
   migration scripts.

2. **Column naming.** C# entity property `UserName` → SQL column `user_name`. EF applies
   the `SnakeCaseColumnNameConvention` automatically. Don't override per-column.

3. **Money columns** use `decimal(18,2)`. All DTOs return `decimal` (not `double`).

4. **JWT secret must be set** in `Web.config` before the app starts. `Global.asax.Application_Start`
   throws if missing. Don't add a fallback default — fail loud is intentional.

5. **Programme data is read-only through ProgrammesController.** New event/campaign fields go on
   `Campaign`, not `Programme`.

6. **Frontend never holds raw cards.** Donations are tokenised via `PaymentToken` (see
   `DonatePage.js`); we never accept `CardNumber`/`CVV` from the browser for PCI-DSS reasons.

7. **Snake_case in DB, PascalCase in C#, camelCase in JSON.** Don't mix them up.

---

## 9. Where to start when adding a feature

| Feature type | Start here |
|---|---|
| New public page | `App.js` (add route), create file in `pages/`, use shared components |
| New admin section | Create file in `pages/admin/`, wrap in `AdminPageFrame`, add route in `App.js` |
| New API endpoint | Add method to existing controller or create new one in `Controllers/`, add DTO in `ViewModels.cs`, register route |
| New DB table | Add migration `*.sql` in project root, add entity in `EntityModels.cs`, add DbSet in `GiveAIDContext.cs` |
| New style | Use existing CSS custom properties (`--c4k-teal`, etc.); check `styles/global.css` |
| New global state | Extend `AuthContext` or create a new context in `contexts/` |
