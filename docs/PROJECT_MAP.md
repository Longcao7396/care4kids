# Project Map — Annotated File Index

> Audience: developers and AI coding assistants. Every file worth editing is listed here
> with a one-line description. Read this **before** searching the codebase — it's faster.

---

## Top-level

| File / Folder         | What it is                                                        |
|-----------------------|-------------------------------------------------------------------|
| `GiveAID.V2.slnx`     | .NET solution (XML format). Open in Rider/VS/VS Code.             |
| `README.md`           | Project entry point — quick start, status, links.                 |
| `src/`                | .NET source — Domain, Application, Infrastructure, WebApi, Web.   |
| `tests/`              | xUnit test projects (4 projects, 169 tests).                      |
| `GiveAID.Client/`     | React 18 public site (separate repo concept, sibling directory).  |
| `database/`           | SQL migration scripts + seeds.                                    |
| `docs/`               | This documentation set.                                           |
| `scripts/`            | PowerShell helpers (`pre-start.ps1`, `start-*.ps1`).              |
| `wireframes/`         | Design reference (PNG/SVG mockups).                               |
| `.github/`            | GitHub workflows (CI templates, future).                          |
| `.gitignore`          | Standard .NET + Node ignores.                                     |

---

## Backend — `src/Domain/`

Pure C# entities, value objects, enums. **No NuGet dependencies**, no `using System.*`
beyond BCL.

### `Entities/` (24 files)

| File                          | Purpose                                                |
|-------------------------------|--------------------------------------------------------|
| `BaseEntity.cs`               | Abstract base with `Id`, `CreatedAt`, `UpdatedAt`      |
| `User.cs`                     | Identity, roles, password hash, lock state             |
| `Cause.cs`                    | Donation cause (hierarchical)                          |
| `Campaign.cs`                 | Time-bound campaign with progress tracking             |
| `CampaignRegistration.cs`     | User sign-up for registration campaigns                |
| `CampaignReport.cs`           | Post-campaign impact reports                           |
| `Donation.cs`                 | Donation transactions                                  |
| `Gallery.cs`                  | Photo gallery items                                    |
| `TeamMember.cs`               | About-us team                                          |
| `Achievement.cs`              | About-us milestones                                    |
| `Career.cs`                   | Job postings                                           |
| `CareerApplication.cs`        | Job applications                                       |
| `Organization.cs`             | Partner organizations                                  |
| `Faq.cs`                      | FAQ entries                                            |
| `ContactMessage.cs`           | Contact form submissions                               |
| `Conversation.cs`             | User-to-admin conversation thread                      |
| `ConversationMessage.cs`      | Message in a conversation                              |
| `Invitation.cs`               | Referral invitations                                   |
| `CmsPage.cs`                  | Editable CMS page content                              |
| `EmailLog.cs`                 | Outbound email audit                                   |
| `WebhookLog.cs`               | Stripe webhook delivery log                            |
| `Programme.cs`                | Legacy alias (merged into Campaign)                    |
| `ProgrammePhoto.cs`           | Legacy alias                                           |
| `ProgrammeRegistration.cs`    | Legacy alias                                           |

### `Enums/` (10 files)

`CampaignStatus`, `DonationStatus`, `PaymentMethod`, `UserRole`, `GalleryCategory`,
`CareerType`, `ContactStatus`, `ConversationStatus`, `EmailStatus`, `WebhookStatus`.

### `Common/` and `ValueObjects/`

- `Result.cs` — `Result<T>` for command outcomes
- `Money.cs`, `Address.cs` — value objects

---

## Application — `src/Application/`

### Top-level

| File                                  | Purpose                                          |
|---------------------------------------|--------------------------------------------------|
| `GiveAID.V2.Application.csproj`       | MSBuild project — depends on `Domain` only       |
| `GlobalUsings.cs`                     | Common `using` imports (MediatR, FluentValidation) |
| `DependencyInjection.cs`              | `AddApplicationServices()` extension method      |

### `Common/`

| File / Folder              | Purpose                                          |
|----------------------------|--------------------------------------------------|
| `Behaviors/`               | MediatR pipeline behaviors (Validation, Logging, Performance) |
| `Exceptions/`              | `ValidationException`, `NotFoundException`, etc. |
| `Mappings/`                | AutoMapper profiles                              |
| `Models/`                  | `PagedResult<T>`, `Result<T>`                    |
| `Interfaces/`              | `IApplicationDbContext`, `IJwtTokenService`, ... |

### `Features/` (one folder per feature)

```
Features/
├── Auth/
│   ├── Commands/
│   │   ├── Login/        { LoginCommand.cs, LoginCommandHandler.cs, LoginCommandValidator.cs }
│   │   ├── Register/
│   │   └── Refresh/
│   └── Queries/
│       └── Me/
├── Campaigns/
│   ├── Commands/
│   │   ├── CreateCampaign/
│   │   ├── UpdateCampaign/
│   │   └── DeleteCampaign/
│   └── Queries/
│       ├── GetCampaigns/
│       └── GetCampaignById/
├── Donations/             (same shape)
├── Causes/                (same shape)
├── Gallery/               (same shape)
├── ... (one folder per controller)
```

### `Services/`

Cross-cutting helpers used by handlers — `DateTimeService`, `CurrentUserAccessor`, etc.

---

## Infrastructure — `src/Infrastructure/`

### `Persistence/`

| File / Folder                       | Purpose                                |
|-------------------------------------|----------------------------------------|
| `GiveAIDDbContext.cs`               | EF Core 8 DbContext                    |
| `Configurations/`                   | `IEntityTypeConfiguration<T>` per entity |
| `Migrations/`                       | EF Core migrations (auto-generated)    |
| `Seed/DatabaseSeeder.cs`            | Seeds admin user + demo data           |
| `Seed/RoleSeeder.cs`                | Seeds roles                             |

### `Security/`

| File                | Purpose                                  |
|---------------------|------------------------------------------|
| `JwtTokenService.cs`| HS256 token issue + validate             |
| `PasswordHasher.cs` | PBKDF2 SHA-256 with random salt          |

### `Email/`

| File                          | Purpose                          |
|-------------------------------|----------------------------------|
| `SmtpEmailService.cs`         | System.Net.Mail SMTP sender      |
| `EmailTemplateRenderer.cs`    | Razor template rendering         |
| `Templates/`                  | `.cshtml` email templates        |

### `Payment/`

| File                       | Purpose                            |
|----------------------------|------------------------------------|
| `StripePaymentService.cs`  | PaymentIntent + webhook signature  |
| `StripeWebhookHandler.cs`  | Processes Stripe events            |

### `Caching/`

| File                       | Purpose                          |
|----------------------------|----------------------------------|
| `MemoryCacheService.cs`    | IMemoryCache wrapper with TTL + tags |

### Top-level

| File                                  | Purpose                                |
|---------------------------------------|----------------------------------------|
| `GiveAID.V2.Infrastructure.csproj`    | MSBuild — refs Domain + Application    |
| `DependencyInjection.cs`              | `AddInfrastructureServices(IConfiguration)` |

---

## WebApi — `src/WebApi/`

### Entry

| File                          | Purpose                                          |
|-------------------------------|--------------------------------------------------|
| `Program.cs`                  | App startup, DI, middleware pipeline            |
| `appsettings.json`            | Connection strings, JWT, SMTP, Stripe, CORS     |
| `appsettings.Development.json`| Dev overrides (verbose logging)                  |
| `GiveAID.V2.WebApi.csproj`    | MSBuild — refs Application + Infrastructure     |
| `GiveAID.V2.WebApi.http`      | `.http` file for testing endpoints in Rider/VS  |

### `Controllers/` (24 controllers)

See [API Reference](API_REFERENCE.md) for the full list.

| Controller                        | Route prefix                |
|-----------------------------------|-----------------------------|
| `HealthController.cs`             | `/api/v1/health`            |
| `AuthController.cs`               | `/api/v1/auth`              |
| `AuthBootstrapController.cs`      | `/api/v1/auth-bootstrap`    |
| `UsersController.cs`              | `/api/v1/users`             |
| `CausesController.cs`             | `/api/v1/causes`            |
| `CampaignsController.cs`          | `/api/v1/campaigns`         |
| `CampaignReportsController.cs`    | `/api/v1/campaign-reports`  |
| `DonationsController.cs`          | `/api/v1/donations`         |
| `GalleryController.cs`            | `/api/v1/gallery`           |
| `TeamController.cs`               | `/api/v1/team`              |
| `AchievementsController.cs`       | `/api/v1/achievements`      |
| `OrganizationsController.cs`      | `/api/v1/supporters`        |
| `CareersController.cs`            | `/api/v1/careers`           |
| `CareerApplicationsController.cs` | `/api/v1/careers/{id}/applications` |
| `FaqsController.cs`               | `/api/v1/faqs`              |
| `ContactsController.cs`           | `/api/v1/contacts`          |
| `ConversationsController.cs`      | `/api/v1/conversations`     |
| `InvitationsController.cs`        | `/api/v1/invitations`       |
| `CmsPagesController.cs`           | `/api/v1/cms/pages`         |
| `StatisticsController.cs`         | `/api/v1/statistics`        |
| `AdminDashboardController.cs`     | `/api/v1/admin/dashboard`   |
| `AdminPaymentsController.cs`      | `/api/v1/admin/payments`    |
| `AdminEmailLogsController.cs`     | `/api/v1/admin/emails`      |
| `AdminUsersController.cs`         | `/api/v1/admin/users`       |

### `Middleware/`

| File                              | Purpose                              |
|-----------------------------------|--------------------------------------|
| `ExceptionHandlingMiddleware.cs`  | Catches unhandled exceptions, returns envelope error |

---

## Web (Admin) — `src/Web/`

ASP.NET Core 8 MVC + Razor. AdminLTE 3.2 UI kit. **No DbContext** — talks to WebApi via HttpClient.

### Entry

| File                          | Purpose                                          |
|-------------------------------|--------------------------------------------------|
| `Program.cs`                  | DI, cookie auth, session, ApiClient              |
| `appsettings.json`            | `Api:BaseUrl = http://localhost:5231`            |

### `Areas/Admin/Controllers/`

| Controller                          | Purpose                              |
|-------------------------------------|--------------------------------------|
| `AuthController.cs`                 | Login, logout, access denied         |
| `DashboardController.cs`            | Stats overview                       |
| `CampaignsController.cs`            | Full CRUD + delete                   |
| `DonationsController.cs`            | List + details                       |
| `CausesController.cs`               | Full CRUD + delete                   |
| `UsersController.cs`                | List + create + edit                 |
| `GalleryController.cs`              | Stub                                 |
| `ReportsController.cs`              | Stub                                 |
| `CareersController.cs`              | Stub                                 |
| `FaqsController.cs`                 | Stub                                 |
| `TeamController.cs`                 | Stub                                 |
| `AchievementsController.cs`         | Stub                                 |
| `OrganizationsController.cs`        | Stub                                 |
| `CmsPagesController.cs`             | Stub                                 |
| `EmailLogsController.cs`            | Stub                                 |
| `ConversationsController.cs`        | Stub                                 |
| `SettingsController.cs`             | Stub                                 |

### `Areas/Admin/Views/`

```
Areas/Admin/Views/
├── Shared/
│   ├── _Layout.cshtml         (AdminLTE master layout)
│   ├── _LoginLayout.cshtml
│   └── _Sidebar.cshtml
├── Auth/
│   ├── Login.cshtml
│   └── AccessDenied.cshtml
├── Dashboard/Index.cshtml
├── Campaigns/
│   ├── Index.cshtml           (DataTable list)
│   ├── Create.cshtml          (form)
│   ├── Edit.cshtml            (form populated from API)
│   └── Details.cshtml
├── ... (similar folders for each entity)
```

### `Services/`

| File                  | Purpose                                                |
|-----------------------|--------------------------------------------------------|
| `ApiClient.cs`        | HttpClient wrapper — auto JWT, envelope unwrap         |
| `AdminSession.cs`     | Session-based admin state (UserId, Email, Role, JWT)   |
| `LoginResponse.cs`    | DTO matching `/auth/login` response shape              |
| `ApiModels.cs`        | `ApiEnvelope<T>`, `PaginatedResult<T>`                 |

### `wwwroot/`

Static assets (CSS, JS, fonts) — minimal; AdminLTE is loaded from CDN.

---

## Tests — `tests/`

| Project                                         | Tests | Purpose                                  |
|-------------------------------------------------|-------|------------------------------------------|
| `Domain.UnitTests/`                             | 71    | Entity business rules                    |
| `Application.UnitTests/`                        | 43    | CQRS handlers + FluentValidation rules   |
| `Infrastructure.IntegrationTests/`              | 28    | PasswordHasher, JwtTokenService, Cache   |
| `WebApi.FunctionalTests/`                       | 27    | HTTP endpoints with WebApplicationFactory |

Each project follows the layout:

```
<Project>/
├── <Project>.csproj
├── Fixtures/         (test fixtures + shared setup)
├── <Area>/           (tests grouped by feature)
└── Smoke/            (sanity tests)
```

---

## Frontend — `GiveAID.Client/`

React 18 SPA. **Not part of the .slnx** — sibling directory.

### Entry

| File                          | Purpose                                    |
|-------------------------------|--------------------------------------------|
| `package.json`                | Deps + scripts (`start`, `build`, `test`)  |
| `src/index.js`                | ReactDOM render root                       |
| `src/App.js`                  | Top-level router                            |
| `src/config.js`               | API base URL + endpoint catalogue          |

### `src/services/`

| File                       | Purpose                                          |
|----------------------------|--------------------------------------------------|
| `api.js`                   | Axios instance + interceptors (JWT, envelope)    |
| `authService.js`           | Login / register / me                            |
| `statisticsService.js`     | Statistics endpoints                             |
| `stripeService.js`         | Stripe.js integration helpers                    |
| `index.js`                 | Barrel re-export                                 |

### `src/pages/` (≈ 30 pages)

`HomePage`, `CampaignsPage`, `CampaignDetailPage`, `CausesPage`, `DonatePage`,
`AboutPage`, `ContactPage`, `CareerPage`, `HelpCentrePage`, `GalleryPage`,
`MyDonationsPage`, `DonationHistoryDetailPage`, `DonationReceiptPage`,
`LoginPage`, `RegisterPage`, `ForgotPasswordPage`, `ResetPasswordPage`,
`PrivacyPage`, `TermsPage`, and a folder `admin/` with admin pages.

---

## Database — `database/`

| Folder            | Purpose                                                  |
|-------------------|----------------------------------------------------------|
| `migrations/`     | Numbered SQL scripts applied in order                    |
| `seeds/`          | Reference data for local development                     |
| `archive/`        | Legacy v1 scripts kept for traceability                 |

---

## Documentation — `docs/`

| File                       | Purpose                                            |
|----------------------------|----------------------------------------------------|
| `README.md`                | Docs index                                         |
| `ARCHITECTURE.md`          | Clean Architecture overview                        |
| `API_REFERENCE.md`         | REST endpoint catalogue                            |
| `DATABASE.md`              | ER diagram + migration order                       |
| `PROJECT_MAP.md`           | This file                                          |
| `CONVENTIONS.md`           | Coding standards + Git workflow                    |
| `MIGRATION_GUIDE.md`       | v1 → v2 migration steps                            |
| `TESTING.md`               | Test strategy + how to run                         |
| `DEPLOYMENT.md`            | Local + production deploy + runbook                |
