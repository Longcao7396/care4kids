# GiveAID v2.0 — Architecture

## 1. Goals

The v2.0 rewrite replaces the legacy ASP.NET WebForms stack (a single monolithic project
with EF6, IIS Express, and inline ADO.NET calls) with a Clean Architecture solution that:

- Enforces **strict layer boundaries** so the domain never depends on infrastructure
- Standardises **CQRS** through MediatR so every use case is a Command or Query
- Provides a **predictable JSON contract** (`{success, message, data, errors?}`) for every
  API response, simplifying client-side error handling
- Ships with **automated tests** (169 tests, 100% pass) covering entities, validators,
  handlers, security helpers, cache, and HTTP endpoints

## 2. Layer Overview

```
                     ┌─────────────────────────────────────────┐
                     │            React 18 Client              │
                     │  (public site, port 3000, axios)        │
                     └────────────────────┬────────────────────┘
                                          │  HTTP/JSON
                                          │  /api/v1/*
                     ┌────────────────────▼────────────────────┐
                     │      ASP.NET Core 8 WebApi              │
                     │  Controllers · Middleware · Scalar      │
                     └────────────────────┬────────────────────┘
                                          │
                     ┌────────────────────▼────────────────────┐
                     │       Application Layer (CQRS)          │
                     │  MediatR · FluentValidation · DTOs      │
                     └────────────────────┬────────────────────┘
                                          │
                     ┌────────────────────▼────────────────────┐
                     │   Infrastructure Layer                  │
                     │  EF Core · JWT · Email · Stripe · Cache │
                     └────────────────────┬────────────────────┘
                                          │
                     ┌────────────────────▼────────────────────┐
                     │       Domain Layer (pure POCOs)         │
                     │   Entities · Value Objects · Enums      │
                     └─────────────────────────────────────────┘
```

## 3. Dependency Rules

The dependency direction is **strictly inward**:

```
WebApi ──▶ Application ──▶ Domain
   │           │
   │           ▼
   └──▶ Infrastructure ──▶ Domain
Web  ──▶ (calls WebApi via HttpClient, never touches DbContext)
```

**Forbidden dependencies:**

- `Domain` may **never** reference any other project.
- `Application` may reference `Domain` only.
- `Infrastructure` may reference `Domain` and `Application`.
- `WebApi` may reference `Application` and `Infrastructure`.
- `Web` (admin console) may **not** reference `Infrastructure` directly — it talks to the
  API through `Services.ApiClient` (`HttpClient`).

These rules are enforced by code review and by the `ProjectReference` graph in the
`.slnx` file (no circular references exist).

## 4. Domain Layer (`src/Domain/`)

Pure C# entities, value objects, and enums. No EF Core attributes, no JSON attributes, no
logging. The domain knows nothing about persistence, transport, or frameworks.

### Entity Catalogue (24 entities)

| Entity                 | Purpose                                                |
|------------------------|--------------------------------------------------------|
| `User`                 | Identity, roles, login metadata                        |
| `Cause`                | Donation cause categories                              |
| `Campaign`             | Time-bound fundraising campaigns                      |
| `CampaignRegistration` | User sign-ups for non-donation campaigns              |
| `CampaignReport`       | Post-campaign impact/transparency reports              |
| `Donation`             | Donation transactions with payment metadata           |
| `Gallery`              | Photo gallery items                                    |
| `TeamMember`           | About-us team page                                     |
| `Achievement`          | About-us milestones                                    |
| `Career`               | Job postings                                           |
| `CareerApplication`    | Job applications                                       |
| `Organization`         | Partner organizations                                  |
| `Faq`                  | Help-centre FAQ entries                                |
| `ContactMessage`       | Contact form submissions                               |
| `Conversation`         | User-to-admin conversation threads (raise-query)      |
| `ConversationMessage`  | Messages inside a conversation                         |
| `Invitation`           | Invite-friends referral tracking                       |
| `CmsPage`              | Editable CMS page content                              |
| `EmailLog`             | Outbound email audit log                               |
| `WebhookLog`           | Stripe webhook delivery log                            |
| `Programme`            | Legacy alias retained for migration compatibility     |
| `ProgrammePhoto`       | Legacy gallery grouping                                |
| `ProgrammeRegistration`| Legacy registration alias                              |
| `BaseEntity`           | Abstract base with `Id`, `CreatedAt`, `UpdatedAt`      |

### Value Objects

- `Money` — currency-safe value type
- `Address` — postal address (used by organizations)

### Enums (10)

`CampaignStatus`, `DonationStatus`, `PaymentMethod`, `UserRole`, `GalleryCategory`,
`CareerType`, `ContactStatus`, `ConversationStatus`, `EmailStatus`, `WebhookStatus`.

## 5. Application Layer (`src/Application/`)

Implements all use cases via **CQRS with MediatR**. Each feature folder contains
`Commands/`, `Queries/`, `Validators/`, and `Handlers/`.

### Folder Structure

```
src/Application/
├── Common/
│   ├── Behaviors/         # MediatR pipeline behaviors
│   ├── Exceptions/        # ValidationException, NotFoundException
│   ├── Mappings/          # AutoMapper profiles
│   └── Models/            # PagedResult<T>, Result<T>
├── Features/
│   ├── Auth/
│   │   ├── Commands/Login/
│   │   │   ├── LoginCommand.cs
│   │   │   ├── LoginCommandHandler.cs
│   │   │   └── LoginCommandValidator.cs
│   │   └── Queries/Me/
│   ├── Campaigns/
│   │   ├── Commands/CreateCampaign/
│   │   ├── Commands/UpdateCampaign/
│   │   ├── Commands/DeleteCampaign/
│   │   └── Queries/GetCampaigns/
│   ├── Donations/
│   ├── Causes/
│   ├── ... (one folder per feature)
├── Interfaces/            # IApplicationDbContext, IJwtTokenService, IEmailService
├── Services/              # Cross-cutting helpers
└── DependencyInjection.cs # AddApplicationServices()
```

### Patterns

- **Command/Query separation** — write operations return `Result<T>`; reads return `T`
  directly (no Result wrapping for read paths)
- **FluentValidation** — every command has a paired `*Validator` registered automatically
  through `AddValidatorsFromAssembly`
- **AutoMapper** — DTO ↔ Entity mapping centralised in `Common/Mappings/`
- **Behaviors** — `ValidationBehavior`, `LoggingBehavior`, `PerformanceBehavior` run in
  pipeline order before the handler

## 6. Infrastructure Layer (`src/Infrastructure/`)

Concrete implementations of the interfaces declared in `Application.Interfaces`.

```
src/Infrastructure/
├── Persistence/
│   ├── GiveAIDDbContext.cs        # EF Core 8 DbContext, fluent API config
│   ├── Configurations/            # IEntityTypeConfiguration<T> per entity
│   ├── Migrations/                # EF Core generated migrations
│   └── Seed/                      # DatabaseSeeder, RoleSeeder
├── Security/
│   ├── JwtTokenService.cs         # Issues/validates JWTs
│   └── PasswordHasher.cs          # PBKDF2 SHA-256 with random salt
├── Email/
│   ├── SmtpEmailService.cs        # System.Net.Mail SMTP sender
│   └── EmailTemplateRenderer.cs   # Razor-light template rendering
├── Payment/
│   ├── StripePaymentService.cs    # PaymentIntent + webhook signature verify
│   └── PaymentResult.cs
├── Caching/
│   └── MemoryCacheService.cs      # IMemoryCache wrapper with TTL + tags
└── DependencyInjection.cs         # AddInfrastructureServices(IConfiguration)
```

### Cross-cutting Concerns

- **Logging** — `ILogger<T>` injected everywhere; structured logging with named placeholders
- **Configuration** — `IOptions<T>` pattern for `JwtSettings`, `SmtpSettings`, `StripeSettings`
- **Resilience** — `HttpClient` typed clients with retry policies (where applicable)

## 7. WebApi Layer (`src/WebApi/`)

Thin transport layer that maps HTTP requests to MediatR calls and shapes responses.

### Middleware Pipeline (order matters)

```
1. ExceptionHandlingMiddleware        ← catches all unhandled exceptions
2. UseCors("ReactDev")                ← CORS before auth
3. UseHttpsRedirection (prod only)
4. UseAuthentication                  ← JWT bearer
5. UseAuthorization
6. UseIpRateLimiting                  ← 100 req/min/IP
7. MapControllers
8. MapHealthChecks("/healthz")
9. MapOpenApi()                       ← Scalar UI
```

### Envelope Contract

Every successful response is wrapped:

```json
{ "success": true, "message": "OK", "data": <payload> }
```

Every error response:

```json
{ "success": false, "message": "Validation failed", "errors": { "field": ["msg"] } }
```

### Controller Catalogue (24 controllers)

| Controller                | Route prefix                | Notes                              |
|---------------------------|-----------------------------|------------------------------------|
| `HealthController`        | `/api/v1/health`            | Liveness + version                 |
| `AuthController`          | `/api/v1/auth`              | Login, register, refresh, me       |
| `AuthBootstrapController` | `/api/v1/auth-bootstrap`    | First-run admin bootstrap          |
| `UsersController`         | `/api/v1/users`             | Profile read/update                |
| `CausesController`        | `/api/v1/causes`            | CRUD, tree, stats                  |
| `CampaignsController`     | `/api/v1/campaigns`         | CRUD, featured, register           |
| `CampaignReportsController` | `/api/v1/campaign-reports` | Impact reports                     |
| `DonationsController`     | `/api/v1/donations`         | Create, list, stats                |
| `GalleryController`       | `/api/v1/gallery`           | Photo gallery CRUD                 |
| `TeamController`          | `/api/v1/team`              | About-us team                      |
| `AchievementsController`  | `/api/v1/achievements`      | Milestones                         |
| `OrganizationsController` | `/api/v1/supporters`        | Partner organizations              |
| `CareersController`       | `/api/v1/careers`           | Job postings                       |
| `CareerApplicationsController` | `/api/v1/careers/{id}/applications` | Applications     |
| `FaqsController`          | `/api/v1/faqs`              | Help-centre FAQ                    |
| `ContactsController`      | `/api/v1/contacts`          | Contact form                       |
| `ConversationsController` | `/api/v1/conversations`     | User-to-admin messaging            |
| `InvitationsController`   | `/api/v1/invitations`       | Referral system                    |
| `CmsPagesController`      | `/api/v1/cms/pages`         | Editable CMS                       |
| `StatisticsController`    | `/api/v1/statistics`        | Dashboard + home aggregates        |
| `AdminDashboardController`| `/api/v1/admin/dashboard`   | Admin overview                     |
| `AdminPaymentsController` | `/api/v1/admin/payments`    | Stripe reconciliation              |
| `AdminEmailLogsController`| `/api/v1/admin/emails`      | Outbound email audit               |
| `AdminUsersController`    | `/api/v1/admin/users`       | Admin user management              |

## 8. Web Layer (`src/Web/`) — Admin Console

Server-rendered admin back-office built on **ASP.NET Core 8 MVC + Razor Pages** with
**AdminLTE 3.2** as the UI kit.

### Authentication Flow

```
Browser ──▶ /Admin/Auth/Login (GET)
       ◀── login form (AdminLTE)
Browser ──▶ /Admin/Auth/Login (POST)
       ──▶ AuthController.Login()
              ──▶ ApiClient.PostAnonymousAsync("auth/login", body)
              ◀── {token, userId, email, role, ...}
       ──▶ HttpContext.SignInAsync("AdminCookie", principal)
       ──▶ Session["Token"] = jwt
       ◀── 302 → /Admin/Dashboard
```

### Architecture

- **No DbContext** in Web. All data fetched through `Services.ApiClient` (HttpClient)
- **Cookie auth** (`AdminCookie`) holds the principal; **session** (`GiveAID.Admin.Session`)
  holds the JWT for API calls
- **Bearer header** attached to every API call by `ApiClient.CreateClient()`

### Sidebar Menu (16 sections)

Dashboard · Causes · Campaigns · Campaign Reports · Donations · Gallery · Users ·
Organizations · Team · Achievements · Careers · FAQs · CMS Pages · Conversations ·
Email Logs · Settings

## 9. React Client (`GiveAID.Client/`)

Public-facing SPA. **Not part of the .NET solution** — lives in a sibling directory and
communicates with the WebApi over HTTP/JSON.

### Stack

- React 18, React Router 6
- Axios (instance with interceptors)
- CSS Modules + plain CSS (no Tailwind)
- Bootstrap is **not** used (AdminLTE is admin-only)

### Axios Interceptor Pattern

```javascript
// src/services/api.js
const api = axios.create({ baseURL: 'http://localhost:5231/api/v1' });

api.interceptors.request.use(config => {
  const token = localStorage.getItem('giveaid_token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  response => {
    // Unwrap {success, message, data} envelope
    if (response.data && 'success' in response.data) {
      if (!response.data.success) return Promise.reject(new Error(response.data.message));
      return response.data.data;
    }
    return response.data;
  },
  error => {
    if (error.response?.status === 401) {
      localStorage.removeItem('giveaid_token');
      window.location.href = '/login';
    }
    return Promise.reject(new Error(error.response?.data?.message || error.message));
  }
);
```

## 10. Database

SQL Server 2019+ (Express OK). Schema managed through **EF Core migrations** under
`src/Infrastructure/Persistence/Migrations/`. Reference seed scripts in `database/seeds/`.

Key tables: `Users`, `Causes`, `Campaigns`, `CampaignRegistrations`, `CampaignReports`,
`Donations`, `Galleries`, `TeamMembers`, `Achievements`, `Careers`, `CareerApplications`,
`Organizations`, `Faqs`, `ContactMessages`, `Conversations`, `ConversationMessages`,
`Invitations`, `CmsPages`, `EmailLogs`, `WebhookLogs`.

## 11. Cross-cutting Concerns

- **JWT** — HS256, secret loaded from `Jwt:Secret`, 60-min lifetime, refresh via `/auth/refresh`
- **Rate limiting** — 100 requests per minute per IP (configurable in `appsettings.json`)
- **CORS** — allow-list in `Cors:AllowedOrigins`; default `localhost:3000`, `localhost:3001`
- **Health checks** — `/healthz` (no auth) for load balancer probes
- **OpenAPI** — `MapOpenApi()` exposes the spec; Scalar UI at `/scalar/v1` in Development

## 12. Dependency Injection Cheat Sheet

```csharp
// Program.cs
builder.Services.AddApplicationServices();        // MediatR, validators, mappers
builder.Services.AddInfrastructureServices(cfg); // DbContext, JWT, Email, Stripe, Cache
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(/* … */);
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("Admin", p => p.RequireRole("Admin"));
    // SuperAdmin policy removed — Admin has full privileges
});
```

## 13. Where to Read Next

- [API Reference](API_REFERENCE.md) — endpoint-by-endpoint contract
- [Database Schema](DATABASE.md) — ER diagram and migration order
- [Testing Strategy](TESTING.md) — how we test this architecture
- [Migration Guide](MIGRATION_GUIDE.md) — moving data from v1
