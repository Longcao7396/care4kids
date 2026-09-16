# Project Map — Annotated File Index

> Audience: AI coding assistants. Every file an AI might want to edit is listed here with a
> one-line description of what it does. Read this **before** searching the codebase — it's
> faster.

---

## Top-level

| File | What it is |
|---|---|
| `GiveAID.Web.sln` | Visual Studio solution |
| `package.json` | Frontend deps + scripts (`npm start` runs both via `concurrently`) |
| `docs/` | This documentation folder |
| `*.sql` (in root) | DB migration scripts (apply in order, see `DATABASE.md`) |

---

## Backend — `GiveAID.Web/`

### Entry points

| File | Role |
|---|---|
| `Global.asax.cs` | App startup. Validates JWT config (fail-fast), runs `SeedDatabase()`, handles CORS preflight. **If something explodes on first request, look here first.** |
| `Web.config` | Connection string + JWT settings + CORS origins. Edit for prod. |
| `packages.config` | NuGet dependencies (legacy format). |
| `GiveAID.Web.csproj` | MSBuild project file (`.NET Framework 4.7.2`, WebApplication project type). |

### `App_Start/`

| File | Role |
|---|---|
| `WebApiConfig.cs` | Web API routes, JSON settings, CORS attribute registration |
| `RouteConfig.cs` | MVC routes (mostly unused — this is API-only) |
| `FilterConfig.cs` | Global filters (error handling) |

### `Controllers/` (the API surface)

| Controller | Routes | Notes |
|---|---|---|
| `AuthController.cs` | `POST /login`, `GET /me`, `POST /register`, `POST /logout` | No `[JwtAuthorize]` — validates JWT manually for `/me` and `/logout` |
| `AuthBootstrapController.cs` | `POST /auth/bootstrap` (SuperAdmin only) | Resets admin + demo password hashes; idempotent |
| `CampaignsController.cs` | CRUD on campaigns; `/featured`, `/my-registrations`, `{id}/register` | Main entity; uses `causeLookup` to avoid N+1 |
| `CausesController.cs` | CRUD on causes; `/tree`, `/{id}/sub-causes`, `/stats` | Hierarchical taxonomy |
| `DonationsController.cs` | CRUD on donations (user's own); `/stats` (admin) | **No card numbers** — `PaymentToken` + `CardLast4` only |
| `InvitationsController.cs` | `/`, `/mine`, `/stats`, `/{id}/cancel`, `/accept/{token}` | Referral system |
| `UsersController.cs` | `GET /me`, `PUT /me` | Authenticated user profile |
| `AchievementsController.cs` | CRUD | About-page impact stats |
| `CareersController.cs` | CRUD + `/apply` | Job board |
| `CmsPagesController.cs` | CRUD by slug | CMS content blocks |
| `ContactsController.cs` | CRUD on contact messages; `/submit` (public) | Contact form |
| `ConversationsController.cs` | CRUD; threaded user↔admin chat | |
| `FaqsController.cs` | CRUD | FAQ entries |
| `GalleryController.cs` | CRUD; `/programmes` (legacy list) | |
| `OrganizationsController.cs` | CRUD on partner NGOs | |
| `SupportersController.cs` | CRUD on supporters | |
| `TeamController.cs` | CRUD on team members | |
| `ProgrammesController.cs` | ⚠️ LEGACY — marked `[Obsolete]`. Use CampaignsController | |
| `AdminDashboardController.cs` | `/stats`, `/recent-donations`, `/user-stats`, `/all-donations` | Uses `JwtHelper.CheckAdmin(Request)` |
| `AdminUsersController.cs` | CRUD on users | |
| `HealthController.cs` | `GET /api/health` | Liveness check |

### `Models/`

| File | Role |
|---|---|
| `EntityModels.cs` | All DB entities (one per table). **Edit this when adding a new table.** |
| `ViewModels.cs` | DTOs for request/response. **Edit this when adding a new API response shape.** |
| `CampaignDto.cs` | Strongly-typed campaign mapping (used by `CampaignsController.MapCampaign`) |

### `Helpers/`

| File | Role |
|---|---|
| `JwtSettings.cs` | Typed accessor for JWT config; throws on missing/invalid values |
| `JwtHelper.cs` | Token generation, validation, role-check (`CheckAdmin(Request)`) |
| `JwtAuthorizeAttribute.cs` | Custom `[JwtAuthorize]` — replaces stock `[Authorize]` (no OWIN needed) |
| `PasswordHasher.cs` | BCrypt wrap (work factor 11) |
| `AuthBootstrap.cs` | One-shot BCrypt hash utility for admin/demo reset |
| `EmailService.cs` | Mock SMTP sender (logs to debug output; swap for real SMTP in prod) |
| `EntityExtensions.cs` | `UpdatedAtSafe()` extension method |

### `Data/`

| File | Role |
|---|---|
| `GiveAIDContext.cs` | EF DbContext. DbSets + snake_case convention + SeedDatabase() |

---

## Frontend — `GiveAID.Client/`

### Entry points

| File | Role |
|---|---|
| `package.json` | Frontend deps + npm scripts |
| `src/index.js` | React root |
| `src/App.js` | **All routes live here** — public + admin. Edit this when adding a page. |
| `src/config.js` | API base URL, endpoint map, status enums |
| `public/index.html` | HTML shell |

### `src/pages/` (public)

| File | Purpose |
|---|---|
| `HomePage.js` | Landing page (hero, donate tiers, featured campaigns, mission pillars) |
| `LoginPage.js` | Login form |
| `RegisterPage.js` | Registration form |
| `CausesPage.js` | 2-level cause taxonomy (parent cause → sub-causes) |
| `CampaignsPage.js` | Campaign listing with filters |
| `CampaignDetailPage.js` | Single campaign detail |
| `DonatePage.js` | Donation form (cause/campaign selection, amount, payment) |
| `DashboardPage.js` | User dashboard |
| `MyDonationsPage.js` | User's donation history |
| `MyRegistrationsPage.js` | User's event registrations |
| `ProfilePage.js` | Profile edit + password change |
| `AboutPage.js` | About us (CMS-driven) |
| `OurTeamPage.js` | Team listing |
| `AchievementsPage.js` | Impact stats |
| `CareerPage.js` | Careers |
| `ContactPage.js` | Contact form |
| `GalleryPage.js` | Photo gallery |
| `HelpCentrePage.js` | FAQ |
| `RaiseQueryPage.js` | User-to-admin query form |
| `SupportersPage.js` | Supporters |
| `OurPartnersPage.js` | Partners |
| `ProgrammesPage.js` | ⚠️ LEGACY — redirects to `/campaigns?eventsOnly=true` |
| `ProgrammeDetailPage.js` | ⚠️ LEGACY — redirects to `/campaigns/:id` |

### `src/pages/admin/`

All admin pages wrap their content in `<AdminPageFrame>` (consistent header + banners).

| File | Purpose |
|---|---|
| `AdminDashboard.js` | KPIs, recent donations, user stats |
| `AdminCampaignPage.js` | Full CRUD on campaigns |
| `AdminCampaignReportsPage.js` | Campaign financial reports + charts |
| `AdminDonationsPage.js` | All donations list |
| `AdminUsersPage.js` | User management |
| `AdminAchievementsPage.js` | Achievements CRUD |
| `AdminQueriesPage.js` | Conversation management |
| `AdminInvitationsPage.js` | Invitations list |
| `AdminNgoPage.js` | NGO CRUD |
| `AdminPartnersPage.js` | Supporters/partners CRUD |
| `AdminGalleryPage.js` | Photo gallery CRUD |
| `AdminCmsPage.js` | Tabbed CMS shell (team, careers, FAQs, etc.) |
| `AdminAboutPage.js` | Same as AdminCmsPage but under `/admin/about` |
| `AdminContactPage.js` | Contact messages |
| `AdminFaqManager.js` | FAQ CRUD (used inside AdminCmsPage tabs) |
| `AdminSiteSettings.js` | Site settings (terms, privacy) |
| `CmsPagesAdmin.js` | CMS pages CRUD |
| `CareersAdmin.js` | Job postings CRUD |
| `SupportersAdmin.js` | Supporters CRUD |
| `TeamAdmin.js` | Team members CRUD |
| `AdminContactInfo.js` | Contact info editor |
| `AdminForm.css` | Shared admin form styling |

### `src/components/` (reusable UI)

| File | Purpose |
|---|---|
| `Navbar.js` | Top navigation |
| `Footer.js` | Footer |
| `AdminPageFrame.js` | **Shared admin page wrapper** — use this for every new admin page |
| `AdminLayout.js` | Admin chrome (sidebar, topbar) |
| `ProtectedRoute.js` | Route guard (auth check) |
| `AuthBootstrap.js` | Listens for `giveaid:auth:expired` events → redirect to login |
| `InviteFriendsModal.js` | Referral invitation modal |

### `src/contexts/`

| File | Purpose |
|---|---|
| `AuthContext.js` | Current user, login/logout, role check |

### `src/services/`

| File | Purpose |
|---|---|
| `api.js` | Axios instance + JWT interceptor + error envelope |
| `index.js` | **All service methods** — `authService`, `campaignsService`, `donationsService`, etc. Add new endpoints here. |

### `src/data/`

| File | Purpose |
|---|---|
| `sampleCampaigns.js` | Static fallback data for offline / first-paint |

### `src/styles/`

| File | Purpose |
|---|---|
| `global.css` | Design tokens (`--c4k-*` CSS custom properties), base resets |

---

## Scripts (PowerShell, in `GiveAID.Client/scripts/`)

| File | Role |
|---|---|
| `start-backend.ps1` | Kills port 44300/61508, then runs IIS Express for `GiveAID.Web` |
| `stop-backend.ps1` | Kills IIS Express / dotnet holding the backend port |
| `start-frontend.ps1` | Kills port 3000, then `npm start` (auto-falls back to 3001) |
| `stop-frontend.ps1` | Kills the frontend dev server |

---

## Where to add what (cheat-sheet)

| I want to… | Edit this |
|---|---|
| Add a public page | `App.js` + new file in `src/pages/` |
| Add an admin page | `App.js` + new file in `src/pages/admin/` (use `AdminPageFrame`) |
| Add an API endpoint | `Controllers/XxxController.cs` + DTO in `Models/ViewModels.cs` + method in `services/index.js` + URL in `config.js` |
| Add a DB table | New entity in `EntityModels.cs` + DbSet in `GiveAIDContext` + new `*.sql` migration in project root |
| Add a new role | Update role-check helpers (`JwtHelper.CheckAdmin` + `[JwtAuthorize(Roles="…")]` call sites) |
| Add a new global style | Add a CSS variable in `styles/global.css` |
| Change the DB connection | `Web.config` → `connectionStrings["GiveAIDContext"]` |
| Change JWT settings | `Web.config` → `appSettings` (`JwtSecret`, `JwtIssuer`, `JwtAudience`, `JwtExpiryMinutes`) |
