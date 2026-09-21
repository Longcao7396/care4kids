# GiveAID Project Architecture Analysis
**Date: 2026-09-18**

## 1. Technology Stack

### Backend
- **Framework:** ASP.NET MVC 5 + Web API 2 (not .NET Core)
- **Target Framework:** .NET Framework 4.7.2
- **ORM:** Entity Framework 6.4.4 (Database-First, SQL script is canonical schema)
- **Database:** Microsoft SQL Server (local: `.\SQLEXPRESS`, DB: `GiveAIDDB`)
- **Authentication:** JWT (JSON Web Tokens) via `System.IdentityModel.Tokens.Jwt`
- **Password Hashing:** BCrypt.Net-Next 4.0.3 (cost factor 11)
- **Payment Gateway:** Stripe (via `GiveAID.Web/Services/Payments/`) + Mock gateway fallback
- **Email:** System.Net.Mail with mock mode (logs to `EmailLogs` table)
- **Rate Limiting:** Custom `RateLimiter` helper class
- **NuGet Packages:** 26 packages managed via `packages.config`

### Frontend
- **Framework:** React 18 (inferred from `useCallback`, `useMemo` patterns)
- **Routing:** React Router DOM v6 (BrowserRouter, Routes, Route)
- **State Management:** React Context API (`AuthContext`) + localStorage
- **HTTP Client:** Axios with request/response interceptors
- **Styling:** Bootstrap 5.1.0 + custom CSS files
- **Build Tool:** Create React App (inferred)
- **Payment:** Stripe.js (lazy-loaded singleton via `stripeService.js`)

---

## 2. Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        GIVEAID ARCHITECTURE                         │
└─────────────────────────────────────────────────────────────────────┘

  ┌──────────────────────────────────────────┐     ┌─────────────────┐
  │          CLIENT (React SPA)               │     │   ADMIN PANEL    │
  │                                          │     │   (same SPA)    │
  │  ┌────────────┐  ┌──────────────────┐   │     │                 │
  │  │ App.js     │  │ Services Layer   │   │     │ /admin/*        │
  │  │ (Router)   │──│  - api.js        │   │     │ (AdminLayout    │
  │  └────────────┘  │  - authService   │   │     │  wrapper)       │
  │       │           │  - stripeService │   │     │                 │
  │       ▼           │  - index.js      │   │     └────────┬────────┘
  │  ┌────────────┐  └────────┬─────────┘   │              │
  │  │ Protected  │           │              │              │
  │  │ Route      │           ▼              │              │
  │  │ (JWT)      │     Axios Interceptors  │              │
  │  └────────────┘     (Bearer Token)      │              │
  │       │                   │              │              │
  └───────┼───────────────────┼──────────────┘              │
          │                   │                              │
          │   HTTPS / REST API │                              │
          ▼                   ▼                              │
  ┌────────────────────────────────────────────────────┐     │
  │              IIS / IIS Express                       │     │
  │         (http://localhost:61508/)                   │     │
  └────────────────────────────┬───────────────────────┘     │
                               │                              │
  ┌────────────────────────────▼───────────────────────┐     │
  │         BACKEND: ASP.NET MVC 5 + Web API 2         │     │
  │                 (.NET Framework 4.7.2)             │     │
  │                                                      │     │
  │  ┌─────────────┐  ┌─────────────┐  ┌────────────┐   │     │
  │  │ Controllers │  │  Helpers    │  │ Services/  │   │     │
  │  │ (24 files)  │  │ (9 files)  │  │ Payments/  │   │     │
  │  │             │  │             │  │ (4 files)  │   │     │
  │  │ AuthController│ │ JwtHelper  │  │            │   │     │
  │  │ DonationsCtrl│ │ EmailSvc   │  │ IPaymentGw │   │     │
  │  │ CampaignsCtrl│ │ RateLimiter│  │ StripePay  │   │     │
  │  │ AdminDash... │ │ PwdHasher  │  │ MockPay    │   │     │
  │  │ + 20 more   │  │ HtmlSanit. │  │ GatewayFac.│   │     │
  │  └──────┬──────┘  └────────────┘  └─────┬──────┘   │     │
  │         │                               │            │     │
  │         ▼                               │            │     │
  │  ┌────────────────────────┐             │            │     │
  │  │  GiveAIDContext (EF6)  │             │            │     │
  │  │  - SnakeCase mapping  │◄────────────┘            │     │
  │  │  - 18 DbSets          │                          │     │
  │  │  - SeedDatabase()     │                          │     │
  │  └───────────┬───────────┘                          │     │
  │              │                                       │     │
  │              ▼                                       │     │
  │  ┌─────────────────────────┐                        │     │
  │  │  EntityModels.cs        │                        │     │
  │  │  (20 entity classes)   │                        │     │
  │  └─────────────────────────┘                        │     │
  └──────────────────────┬──────────────────────────────┘     │
                         │                                     │
                         ▼                                     │
  ┌──────────────────────────────────────────────────────┐    │
  │            SQL SERVER (.\SQLEXPRESS)                  │    │
  │            Database: GiveAIDDB                        │    │
  │                                                      │    │
  │  Users | Causes | Campaigns | Donations              │    │
  │  CampaignRegistrations | CampaignReports            │    │
  │  Organizations | Gallery | ContactMessages          │    │
  │  Careers | CareerApplications | TeamMembers        │    │
  │  Achievements | Faqs | Conversations                │    │
  │  ConversationMessages | Invitations                │    │
  │  WebhookLogs | EmailLogs | CmsPages                │    │
  └──────────────────────────────────────────────────────┘    │
                                                                 │
  ┌──────────────────────────────────────────────────────────┐  │
  │           EXTERNAL SERVICES                               │  │
  │  ┌─────────────┐  ┌──────────────┐  ┌──────────────┐   │  │
  │  │ Stripe API  │  │ SMTP Server  │  │ Stripe.js    │   │  │
  │  │ (Payments)  │  │ (Email)     │  │ (Frontend)   │   │  │
  │  └─────────────┘  └──────────────┘  └──────────────┘   │  │
  └──────────────────────────────────────────────────────────┘  │
```

---

## 3. Backend Architecture

### Controllers (24 total)

| Controller | Route Prefix | Responsibilities |
|---|---|---|
| `AuthController` | `/api/auth` | Login, Register, Me, Logout |
| `UsersController` | `/api/users` | User CRUD (admin) |
| `CausesController` | `/api/causes` | Cause/Category management |
| `CampaignsController` | `/api/campaigns` | Campaign CRUD, registration |
| `CampaignReportsController` | `/api/campaign-reports` | Impact/transparency reports |
| `DonationsController` | `/api/donations` | Donation CRUD, webhook handler |
| `AdminDashboardController` | `/api/admin` | Dashboard stats, donation list |
| `AdminPaymentsController` | `/api/admin/payments` | Admin payment management |
| `StatisticsController` | `/api/statistics` | Analytics endpoints |
| `GalleryController` | `/api/gallery` | Gallery photo management |
| `ContactsController` | `/api/contacts` | Contact form submissions |
| `ConversationsController` | `/api/conversations` | User-admin messaging |
| `InvitationsController` | `/api/invitations` | Invite friends feature |
| `CareersController` | `/api/careers` | Job listings + applications |
| `TeamController` | `/api/team` | Team member management |
| `AchievementsController` | `/api/achievements` | Achievements management |
| `OrganizationsController` | `/api/supporters` | Supporter/organization CRUD |
| `CmsPagesController` | `/api/cms` | CMS page management |
| `FaqsController` | `/api/faqs` | FAQ management |
| `AdminUsersController` | `/api/admin/users` | Admin user management |
| `AdminEmailLogsController` | `/api/admin/emails` | Email log viewing + retry |
| `AuthBootstrapController` | `/api/auth/bootstrap` | Auth state bootstrapping |
| `HealthController` | `/api/health` | Health check |
| `SupportersController` | `/api/supporters` | (alias for Organizations) |

**Pattern Observations:**
- Most controllers use manual JWT validation via `JwtHelper` (no `[Authorize]` attribute)
- Controllers instantiate `GiveAIDContext` directly (no DI container)
- No service layer pattern — business logic lives directly in controllers
- `Dispose()` pattern properly implemented

### Services (Payment Gateway)

| Service | Responsibilities |
|---|---|
| `IPaymentGateway` | Abstract interface for payment gateways |
| `StripePaymentGateway` | Stripe API integration (CreatePaymentIntent, VerifyWebhook) |
| `MockPaymentGateway` | Mock implementation for development/testing |
| `PaymentGatewayFactory` | Factory pattern to select active gateway |

### Helpers (9 files)

| Helper | Responsibilities |
|---|---|
| `JwtHelper` | JWT generation, validation, user ID extraction, admin check |
| `JwtSettings` | JWT config (secret, issuer, audience, expiry) |
| `JwtAuthorizeAttribute` | Custom `[JwtAuthorize]` filter attribute |
| `EmailService` | Centralized email sending (mock + real SMTP), typed helpers |
| `PasswordHasher` | BCrypt wrapper for hashing/verifying passwords |
| `RateLimiter` | Request rate limiting |
| `HtmlSanitizer` | HTML sanitization for user input |
| `EntityExtensions` | EF entity extension methods |
| `AuthBootstrap` | Auth initialization helper |

### Data Layer

- **DbContext:** `GiveAIDContext` (Entity Framework 6)
- **Convention:** `SnakeCaseColumnNameConvention` — C# PascalCase maps to SQL snake_case
- **Key Features:**
  - `Database.SetInitializer<GiveAIDContext>(null)` — SQL script is canonical schema
  - Unique indexes on `Users.Email` and `Users.Username`
  - `Donation.IdempotencyKey` composite unique index
  - `CampaignRegistration` (CampaignId, UserId) unique constraint
  - `SeedDatabase()` creates default Admin + Demo users with BCrypt hashes
  - Decimal precision 18,2 for all monetary fields

### Domain Models (EntityModels.cs — 20 entities)

| Entity | Table | Key Fields |
|---|---|---|
| `User` | Users | UserId, Email, Username, PasswordHash, Role, IsActive, IsVerified, PasswordChangedAt |
| `Cause` | Causes | CauseId, CauseName, TargetAmount, RaisedAmount, ParentCauseId (hierarchical) |
| `Campaign` | Campaigns | CampaignId, CauseId, GoalAmount, RaisedAmount, ProgrammeType, RegistrationRequired |
| `CampaignRegistration` | CampaignRegistrations | RegistrationId, CampaignId, UserId, Status, AttendanceConfirmed |
| `CampaignReport` | CampaignReports | ReportId, CampaignId, TotalReceived, TotalSpent, ExpenseBreakdown (JSON) |
| `Donation` | Donations | DonationId, UserId, Amount, PaymentStatus, GatewayTransactionId, IdempotencyKey, ClientSecret |
| `Organization` | Organizations | OrganizationId, OrganizationName, Mission, Vision, ContributionAmount |
| `Conversation` | Conversations | ConversationId, UserId, Subject, Status, Priority, AssignedTo |
| `ConversationMessage` | ConversationMessages | MessageId, ConversationId, SenderId, MessageText, IsInternalNote |
| `CmsPage` | CmsPages | PageId, PageKey, PageTitle, Content, MetaDescription |
| `Career` | Careers | CareerId, PositionTitle, Requirements, SalaryRange, Vacancies |
| `CareerApplication` | CareerApplications | ApplicationId, CareerId, ApplicantName, ResumeUrl, LinkedInUrl |
| `Gallery` | Gallery | GalleryId, PhotoUrl, Category, Tags, IsFeatured |
| `ContactMessage` | ContactMessages | ContactId, Name, Email, Subject, Message, ReplyMessage |
| `TeamMember` | TeamMembers | TeamMemberId, FullName, RoleTitle, Bio, SocialLinks |
| `Achievement` | Achievements | AchievementId, Title, MetricValue, MetricLabel, AwardBy |
| `Faq` | Faqs | FaqId, Question, Answer, Category, ViewCount |
| `Invitation` | Invitations | InvitationId, InviteeName, InviteeEmail, InvitationToken, Status |
| `WebhookLog` | WebhookLogs | WebhookLogId, Gateway, EventType, EventId, RawPayload, SignatureValid, ProcessingStatus |
| `EmailLog` | EmailLogs | EmailLogId, ToEmail, Subject, Body, Status, ErrorMessage, RetryCount |

---

## 4. Frontend Architecture

### Pages (49 total)

**Public Pages:**
| Page | Route | Description |
|---|---|---|
| `HomePage` | `/` | Homepage with statistics, featured campaigns |
| `LoginPage` | `/login` | User login |
| `RegisterPage` | `/register` | User registration |
| `CausesPage` | `/causes` | Browse cause categories |
| `CampaignsPage` | `/campaigns` | Browse all campaigns |
| `CampaignDetailPage` | `/campaigns/:id` | Campaign detail + donate |
| `AboutPage` | `/about` | About us overview |
| `OurTeamPage` | `/about/team` | Team member profiles |
| `CareerPage` | `/about/careers` | Open positions |
| `AchievementsPage` | `/about/achievements` | Organizational achievements |
| `SupportersPage` | `/about/supporters` | Sponsors & partners |
| `OurPartnersPage` | `/about/partners` | Partner organizations |
| `ContactPage` | `/contact` | Contact form |
| `HelpCentrePage` | `/help-centre` | FAQ + search |
| `GalleryPage` | `/gallery` | Photo gallery |
| `PrivacyPage` | `/privacy` | Privacy policy |
| `TermsPage` | `/terms` | Terms of service |

**Protected User Pages:**
| Page | Route | Description |
|---|---|---|
| `DonatePage` | `/donate` | Donation flow |
| `DonationReceiptPage` | `/donation-receipt/:id` | Donation receipt |
| `DonationHistoryDetailPage` | `/my-donations/:id` | Donation detail |
| `DashboardPage` | `/dashboard` | User dashboard |
| `MyDonationsPage` | `/my-donations` | Donation history |
| `MyRegistrationsPage` | `/my-registrations` | Campaign registrations |
| `ProfilePage` | `/profile` | User profile management |
| `RaiseQueryPage` | `/raise-query` | Submit support query |

**Admin Pages (16):**
| Page | Route |
|---|---|
| `AdminDashboard` | `/admin` |
| `AdminCampaignPage` | `/admin/campaigns` |
| `AdminCampaignReportsPage` | `/admin/campaign-reports` |
| `AdminDonationsPage` | `/admin/donations` |
| `AdminUsersPage` | `/admin/users` |
| `AdminNgoPage` | `/admin/ngos` |
| `AdminPartnersPage` | `/admin/partners` |
| `AdminGalleryPage` | `/admin/gallery` |
| `AdminAchievementsPage` | `/admin/achievements` |
| `AdminCmsPage` | `/admin/cms` |
| `AdminAboutPage` | `/admin/about` |
| `AdminQueriesPage` | `/admin/queries` |
| `AdminContactPage` | `/admin/contacts` |
| `AdminInvitationsPage` | `/admin/invitations` |
| `AdminEmailLogsPage` | `/admin/emails` |

### Services (5 files)

| Service | Description |
|---|---|
| `api.js` | Axios instance with interceptors (auto-retry, backend fallback, 401 handling) |
| `authService.js` | Login, logout, register, token management (dual export with class) |
| `stripeService.js` | Stripe.js lazy loading, card element creation, payment confirmation |
| `statisticsService.js` | Dashboard/homepage statistics endpoints |
| `index.js` | Aggregated exports for all services (causes, donations, campaigns, team, etc.) |

**Key Patterns in Frontend:**
- `AuthContext` — React Context for global auth state with `useCallback` memoization
- `ProtectedRoute` component — wraps protected pages, redirects to `/login` if unauthenticated
- `AdminLayout` — separate layout shell for admin routes (own Navbar/sidebar)
- Axios interceptor: auto-appends `Bearer` token, normalizes PascalCase→camelCase responses
- Backend failover: tries `localhost:44300` then `localhost:61508` if unreachable
- `AuthBootstrap` — handles `giveaid:auth:expired` custom event for soft redirect

### Components

| Component | Description |
|---|---|
| `Navbar` | Public navigation with auth state |
| `Footer` | Site footer |
| `AdminLayout` | Admin panel shell with sidebar |
| `ProtectedRoute` | Route guard with role-based access |
| `AuthBootstrap` | Initializes auth state on app mount |

---

## 5. Database Schema

### Tables (20)

```
Users
  ├── UserId (PK)
  ├── Username (unique)
  ├── Email (unique)
  ├── PasswordHash
  ├── FullName, Phone, Address, Profession
  ├── DateOfBirth, Gender
  ├── Role (User | Admin | SuperAdmin)
  ├── IsActive, IsVerified
  ├── VerificationToken
  ├── LastLogin, PasswordChangedAt
  └── CreatedAt, UpdatedAt

Causes
  ├── CauseId (PK)
  ├── CauseCode
  ├── CauseName, Description, ImageUrl, Icon
  ├── TargetAmount, RaisedAmount
  ├── ParentCauseId (FK → Causes, nullable)
  └── IsActive, DisplayOrder, CreatedAt, UpdatedAt
     └── 1:N → Campaigns

Campaigns
  ├── CampaignId (PK)
  ├── CauseId (FK → Causes)
  ├── OrganizationId (FK → Organizations, nullable)
  ├── CampaignName, CampaignCode
  ├── ProgrammeType, RegistrationRequired, MaxParticipants
  ├── TargetBeneficiaries
  ├── ExpectedBudget, ActualBudget
  ├── Description
  ├── GoalAmount, RaisedAmount
  ├── StartDate, EndDate
  ├── ImageUrl, Location
  ├── BeneficiariesCount
  ├── Status, IsFeatured
  └── CreatedAt, UpdatedAt
     └── 1:N → Donations
     └── 1:N → CampaignRegistrations
     └── 1:N → CampaignReports

CampaignRegistrations
  ├── RegistrationId (PK)
  ├── CampaignId (FK → Campaigns)
  ├── UserId (FK → Users)
  ├── Status
  ├── Notes, AttendanceConfirmed
  └── RegistrationDate

CampaignReports
  ├── ReportId (PK)
  ├── CampaignId (FK → Campaigns)
  ├── TotalReceived, TotalSpent
  ├── BeneficiariesReached
  ├── ReportTitle, ReportContent
  ├── ExpenseBreakdown (JSON)
  ├── Photos (JSON), Documents (JSON)
  ├── IsPublished, PublishedDate, PublishedBy
  └── CreatedAt, UpdatedAt

Donations
  ├── DonationId (PK)
  ├── UserId (FK → Users)
  ├── CauseId (FK → Causes)
  ├── CampaignId (FK → Campaigns, nullable)
  ├── OrganizationId (FK → Organizations, nullable)
  ├── Amount
  ├── PaymentMethod, PaymentStatus
  ├── CardLastFour, CardType
  ├── TransactionId, GatewayTransactionId
  ├── Message
  ├── IsAnonymous, ReceiptSent
  ├── IdempotencyKey (nullable)
  ├── ClientSecret
  ├── PaymentGateway
  └── DonationDate, CreatedAt, PaymentConfirmedAt

Organizations
  ├── OrganizationId (PK)
  ├── OrganizationName, OrganizationType
  ├── Description, LogoUrl
  ├── WebsiteUrl
  ├── ContactEmail, ContactPhone
  ├── Address, RegistrationNumber
  ├── Mission, Vision
  ├── ContributionAmount, ContributionType
  └── IsActive, IsFeatured, DisplayOrder, CreatedAt, UpdatedAt

Conversations
  ├── ConversationId (PK)
  ├── UserId (FK → Users, nullable)
  ├── Subject
  ├── ConversationType
  ├── Status, Priority
  ├── AssignedTo
  └── CreatedAt, UpdatedAt, ClosedAt
     └── 1:N → ConversationMessages

ConversationMessages
  ├── MessageId (PK)
  ├── ConversationId (FK → Conversations)
  ├── SenderId
  ├── MessageText
  ├── IsInternalNote
  ├── Attachments
  └── CreatedAt

CmsPages
  ├── PageId (PK)
  ├── PageKey, PageSlug
  ├── PageTitle
  ├── Content
  ├── MetaDescription, MetaKeywords
  ├── IsActive, IsInMenu
  ├── ParentPageId
  └── DisplayOrder, CreatedAt, UpdatedAt, UpdatedBy

Careers
  ├── CareerId (PK)
  ├── PositionTitle, Department
  ├── Description, Requirements, Responsibilities
  ├── Location
  ├── EmploymentType, SalaryRange
  ├── Vacancies
  ├── PostedDate, ClosingDate
  ├── IsActive
  └── CreatedAt, CreatedBy
     └── 1:N → CareerApplications

CareerApplications
  ├── ApplicationId (PK)
  ├── CareerId (FK → Careers)
  ├── ApplicantName, Email, Phone
  ├── ResumeUrl, LinkedInUrl, PortfolioUrl
  ├── CoverLetter
  ├── Status
  ├── ReviewedBy, ReviewedAt, Notes
  └── AppliedAt

Gallery
  ├── GalleryId (PK)
  ├── Title, PhotoUrl, ThumbnailUrl
  ├── Category, Tags
  ├── OrganizationId, ProgrammeId
  └── IsFeatured, DisplayOrder, UploadedAt, UploadedBy

ContactMessages
  ├── ContactId (PK)
  ├── Name, Email, Phone
  ├── Subject, Message
  ├── IsRead
  ├── RepliedBy, ReplyMessage, RepliedAt
  └── CreatedAt

TeamMembers
  ├── TeamMemberId (PK)
  ├── FullName, RoleTitle, Department
  ├── Bio, PhotoUrl
  ├── Email
  ├── LinkedInUrl, TwitterUrl, FacebookUrl
  └── DisplayOrder, IsActive, IsFeatured, JoinedDate
  └── CreatedAt, UpdatedAt, CreatedBy (FK → Users)

Achievements
  ├── AchievementId (PK)
  ├── Title, Category, Description
  ├── MetricValue, MetricLabel, MetricSuffix
  ├── AchievementDate
  ├── ImageUrl, Icon
  ├── AwardBy, Location
  ├── Beneficiaries
  └── DisplayOrder, IsActive, IsFeatured
  └── CreatedAt, UpdatedAt, CreatedBy (FK → Users)

Faqs
  ├── FaqId (PK)
  ├── Question, Answer
  ├── Category
  └── DisplayOrder, IsActive, IsFeatured, ViewCount
  └── CreatedAt, UpdatedAt, CreatedBy (FK → Users)

Invitations
  ├── InvitationId (PK)
  ├── InviterUserId (FK → Users, nullable)
  ├── InviteeName, InviteeEmail
  ├── PersonalMessage
  ├── Status
  ├── InvitationToken
  ├── SentAt, RegisteredAt, FailureReason
  └── CreatedAt, UpdatedAt

WebhookLogs
  ├── WebhookLogId (PK, bigint)
  ├── Gateway
  ├── EventType, EventId
  ├── RawPayload, Signature
  ├── SignatureValid
  ├── ProcessingStatus
  ├── ErrorMessage
  ├── DonationTransactionId, DonationId
  └── ReceivedAt, ProcessedAt

EmailLogs
  ├── EmailLogId (PK)
  ├── ToEmail, Subject, Body
  ├── Category
  ├── RelatedId
  ├── Status (Sent | Failed | MockSent | PendingRetry)
  ├── SentAt, ErrorMessage
  ├── RetryCount
  └── CreatedAt, UpdatedAt
```

### Key Relationships

```
Users (1) ──── (N) Donations
Users (1) ──── (N) CampaignRegistrations
Users (1) ──── (N) Conversations ──── (N) ConversationMessages
Users (1) ──── (N) Invitations
Causes (1) ──── (N) Campaigns ──── (N) Donations
Causes (1) ──── (N) Campaigns ──── (N) CampaignRegistrations
Causes (1) ──── (N) Campaigns ──── (N) CampaignReports
Campaigns (N) ──── (1) Causes
Campaigns (N) ──── (1) Organizations
Careers (1) ──── (N) CareerApplications
Achievements (N) ──── (1) Users (CreatedBy)
TeamMembers (N) ──── (1) Users (CreatedBy)
Faqs (N) ──── (1) Users (CreatedBy)
```

---

## 6. Security Architecture

### Authentication

- **Type:** JWT Bearer Token (stateless)
- **Algorithm:** HMAC-SHA256 (`SecurityAlgorithms.HmacSha256`)
- **Token Lifetime:** 1440 minutes (24 hours, configurable via `JwtExpiryMinutes`)
- **Token Contents:**
  - `NameIdentifier` → UserId
  - `Name` → Username
  - `Email` → Email
  - `Role` → User role
  - `Jti` → Unique token ID (Guid)
  - `Iat` → Issued-at timestamp
  - `password_at` → PasswordChangedAt Unix seconds (for token invalidation on password change)
- **Validation:**
  - Token expiry check
  - Issuer + Audience validation
  - User must be active (`IsActive = true`)
  - Token `password_at` >= DB `PasswordChangedAt` (forces re-login after password change)
- **Storage:** localStorage (`giveaid_token`, `giveaid_user`)
- **Refresh:** No refresh token — user must re-login after 24h

### Authorization

- **Role-based:** `SuperAdmin`, `Admin`, `ContentManager`, `User`
- **Implementation:** Custom `JwtAuthorize` attribute + `JwtHelper.CheckAdmin()` helper
- **Admin Routes:** Protected by `ProtectedRoute` with `roles={['Admin', 'SuperAdmin']}`
- **IDOR Protection:** `DonationsController.GetById()` returns 404 (not 403) for unauthorized access to other users' donations

### API Security

- **CORS:** Configurable allowlist via `Cors:AllowedOrigins` in Web.config
  - Development: `localhost` allowlist
  - Production: must set explicit origins
- **Rate Limiting:** `RateLimiter` helper class (custom implementation)
- **Input Validation:** Server-side validation in controllers + DataAnnotations on models
- **SQL Injection:** Parameterized queries via EF + raw SQL uses `@p0`, `@p1` placeholders
- **XSS:** `HtmlSanitizer` helper for user-generated HTML content
- **Webhook Verification:** Gateway signature validation (Stripe HMAC-SHA256)

### Payment Security

- **PCI-DSS Compliance:** Raw card data NEVER touches the application — only gateway tokens
- **Idempotency:** `IdempotencyKey` prevents duplicate donations from double-submit/network retry
- **Payment Status Flow:** `Pending` → `Completed` (via webhook or admin manual confirm)
- **Amount Integrity:** Cached `raised_amount` updated via raw SQL `COALESCE(...)+` to prevent race conditions
- **Webhook Deduplication:** EventId uniqueness check in `WebhookLogs`

### Email Security

- **Mock Mode:** Default (`SmtpEnabled=false`) — emails logged to DB, not sent
- **SMTP Credentials:** Must NOT be committed; use environment variables
- **Email Logs:** Full audit trail of all email attempts (Sent/Failed/MockSent/PendingRetry)

---

## 7. Issues & Risks

### Critical Issues

1. **JWT Secret is Placeholder in Production**
   - `Web.config` contains `JwtSecret = "REPLACE_WITH_BASE64_SECRET..."` — default placeholder value
   - All JWT tokens signed with weak/known secret can be forged
   - **Fix:** Set `GIVEAID_JWT_SECRET` environment variable or edit `Web.config` before deployment

2. **Legacy Table References in Code (Technical Debt)**
   - `AdminDashboardController.cs` lines 46-52: `db.Database.SqlQuery<int>("SELECT COUNT(*) FROM dbo.Programmes...")` 
   - `AdminDashboardController.cs` lines 164-170: `db.Database.SqlQuery<int>("SELECT COUNT(*) FROM dbo.ProgrammeRegistrations")`
   - `ProgrammesController.cs` and `ProgrammeDetailPage.js` were deleted but these raw SQL queries remain
   - These will throw SQL errors if the legacy `Programmes` and `ProgrammeRegistrations` tables are dropped

3. **No Dependency Injection Container**
   - Every controller instantiates `new GiveAIDContext()` directly
   - No IoC container (Unity, Autofac, etc.) — hard to test, hard to swap implementations
   - `StripePaymentGateway` instantiated via `PaymentGatewayFactory` static class — also not injectable

4. **Global Exception Handler Missing**
   - No `ExceptionFilter` or global `Application_Error` handler
   - Unhandled exceptions return HTML error pages (not JSON) in production
   - `customErrors mode="Off"` in Web.config — exposes stack traces

5. **Web.config Contains Sensitive Defaults**
   - `SmtpFrom = "no-reply@care4kids.org"` (wrong domain, should be configurable)
   - `PublicSiteUrl = "https://care4kids.org"` (different from GiveAID branding)
   - `SmtpEnabled = false` blocks real email delivery in production if not configured

### Medium Issues

6. **Dual Service Export Pattern (Frontend)**
   - `authService.js` exports both `authService` (class instance) AND `new AuthService()` (default)
   - `services/index.js` also re-exports `authService` wrapping the API directly
   - Two different authentication implementations coexisting — confusing and error-prone
   - Need to consolidate to one canonical implementation

7. **No Caching Layer**
   - Every request hits the database directly
   - Homepage statistics (`StatisticsController`) re-aggregates from scratch on every request
   - No Redis, no in-memory cache — will not scale

8. **No Database Indexes Beyond Primary/Foreign Keys**
   - No composite indexes for common query patterns
   - `Donations.DonationDate` — no index for "last 30 days" queries in `AdminDashboardController`
   - `Donations.PaymentStatus` — no index for status-filtered queries

9. **Frontend API URL Hardcoded to localhost**
   - `config.js` has `BACKEND_CANDIDATES = ['http://localhost:44300/api', 'http://localhost:61508/api']`
   - Must be changed for staging/production deployment
   - No environment variable support (`REACT_APP_API_URL` defined but only used as fallback)

10. **Password Reset / Email Verification Not Fully Implemented**
    - `User.VerificationToken` field exists but no corresponding controller endpoints for email verification
    - Users can register without email verification
    - No password reset / forgot password flow

11. **Campaign RaisedAmount Not Always Accurate**
    - `Campaign.RaisedAmount` and `Cause.RaisedAmount` are cached/denormalized
    - Updated via raw SQL `UPDATE ... SET raised_amount = COALESCE(raised_amount, 0) + @p0`
    - No transaction wrapping between donation insert and amount update — possible inconsistency on failure

### Technical Debt

12. **EF6 on .NET 4.7.2 is Legacy**
    - No async/await at EF level (EF6 doesn't support `await` for LINQ-to-Entities queries)
    - Only `DonationsController` uses `async/await` (for gateway calls)
    - Should migrate to Entity Framework Core for async support

13. **No Unit Tests for Backend**
    - `GiveAID.Tests` project exists in solution but no test files visible in the repo
    - Controllers have no `[TestMethod]` coverage

14. **No Migration Strategy**
    - `Database.SetInitializer<GiveAIDContext>(null)` disables EF migrations entirely
    - Schema changes must be applied manually via SQL scripts
    - Risk of schema drift between environments

15. **Hardcoded Strings Throughout**
    - Payment status values: `"Pending"`, `"Completed"`, `"Failed"`, `"Refunded"` — string constants, not enums
    - Role names: `"Admin"`, `"SuperAdmin"` — string comparisons throughout
    - Email categories: `"invitation"`, `"donation_receipt"` — no constants

16. **SnakeCase Convention Inconsistencies**
    - `CareerApplication.LinkedInUrl` uses `[Column("linkedin_url")]` attribute instead of the convention
    - Some columns may not follow snake_case consistently (depends on SQL migration scripts)

---

## 8. Recommendations

### Short-term (1-3 months)

1. **Set Production JWT Secret** — Generate a strong secret and set via environment variable. Rotate regularly.
2. **Remove Legacy Table References** — Delete the raw SQL queries referencing `Programmes` and `ProgrammeRegistrations` tables, or add proper null checks with fallback to 0.
3. **Add Global Exception Handler** — Implement `ExceptionFilterAttribute` that returns consistent JSON error responses. Set `customErrors mode="RemoteOnly"` in Web.config.
4. **Configure SMTP for Production** — Set `SmtpEnabled=true`, `SmtpHost`, `SmtpUsername`, `SmtpPassword` via environment variables or secure config.
5. **Add Database Indexes** — Add composite indexes for:
   - `Donations(PaymentStatus, DonationDate)` — for recent completed donations
   - `Donations(UserId, DonationDate)` — for user donation history
   - `Campaigns(Status, StartDate, EndDate)` — for active campaign filtering
6. **Consolidate Auth Service** — Remove the duplicate `authService` in `services/index.js`, keep only the class-based implementation in `authService.js`.

### Medium-term (3-6 months)

7. **Implement Dependency Injection** — Introduce Autofac or Unity IoC container. Register `GiveAIDContext` as scoped, register services and gateways.
8. **Add Database Migrations** — Enable EF6 migrations for schema change tracking. Document the migration workflow.
9. **Implement Password Reset Flow** — Add `ForgotPassword` and `ResetPassword` endpoints. Implement email verification on registration.
10. **Add Backend Caching** — Cache homepage statistics with 5-minute TTL (MemoryCache or Redis). Cache cause/campaign lists.
11. **Create Constants/Enums** — Replace all magic strings (payment statuses, roles, email categories) with C# enums and constants.
12. **Add Unit Tests** — Cover JWT validation, password hashing, donation idempotency logic, webhook processing.

### Long-term (6-12 months)

13. **Migrate to ASP.NET Core** — .NET 4.7.2 is in maintenance mode. Migrate to .NET 8 for:
    - Built-in async/await with EF Core
    - Native dependency injection
    - Better performance and cross-platform support
    - Built-in JWT Bearer authentication middleware
    - OpenAPI/Swagger support

14. **Implement Refresh Tokens** — Current JWT has 24h expiry with no refresh. Add refresh token mechanism to avoid forcing re-login.

15. **Add Background Job Processing** — Email retry, webhook processing, scheduled tasks (campaign status updates) need a background worker (Hangfire, Quartz.NET, or Azure Functions).

16. **Implement API Versioning** — Current API has no versioning. Add `api/v1/` prefix for future compatibility.

17. **Add Frontend State Management** — For complex admin state, consider Redux or Zustand. Current Context API is sufficient for auth but admin pages have complex local state.

18. **Set Up CI/CD Pipeline** — GitHub Actions workflow for:
    - Backend: Build, run tests, deploy to Azure App Service
    - Frontend: Build, Lighthouse audit, deploy to Azure Static Web Apps
    - Database: Migration script execution as part of deployment

---

## 9. Architecture Score

| Category | Score | Notes |
|---|---|---|
| **Backend Structure** | 5/10 | No service layer, no DI, direct DbContext instantiation in controllers, legacy SQL references |
| **Frontend Structure** | 7/10 | Good routing, component organization, but duplicate auth service implementations |
| **Database Design** | 7/10 | Solid EF configuration, good entity design, but missing indexes and legacy table references |
| **Security** | 6/10 | JWT implementation is solid, but placeholder secret, no refresh tokens, no global error handling |
| **Scalability** | 3/10 | No caching, no background jobs, direct DB queries without optimization — will not scale |
| **Maintainability** | 6/10 | Good code documentation, but magic strings, no tests, no migrations, inconsistent patterns |
| **Code Quality** | 6/10 | Well-commented code with security notes, but no unit tests, some legacy code, dual patterns |

### Overall: **40/70**

---

## Appendix: Project File Structure

```
C:\Users\admin\Desktop\project NGO\
├── GiveAID.sln                          # Root solution (Web + Tests)
├── GiveAID.Web/
│   ├── GiveAID.Web.csproj              # .NET 4.7.2 ASP.NET MVC 5 project
│   ├── GiveAID.Web.sln                  # Web project solution
│   ├── Web.config                        # App config (DB, JWT, SMTP, CORS)
│   ├── packages.config                   # NuGet packages
│   │
│   ├── App_Start/
│   │   ├── WebApiConfig.cs              # CORS, JSON, routing config
│   │   ├── RouteConfig.cs
│   │   └── FilterConfig.cs
│   │
│   ├── Controllers/                     # 24 controllers
│   │   ├── AuthController.cs
│   │   ├── DonationsController.cs
│   │   ├── CampaignsController.cs
│   │   ├── AdminDashboardController.cs
│   │   ├── AdminEmailLogsController.cs
│   │   ├── AdminPaymentsController.cs
│   │   ├── StatisticsController.cs
│   │   ├── GalleryController.cs
│   │   └── ... (18 more)
│   │
│   ├── Services/
│   │   └── Payments/
│   │       ├── IPaymentGateway.cs
│   │       ├── StripePaymentGateway.cs
│   │       ├── MockPaymentGateway.cs
│   │       └── PaymentGatewayFactory.cs
│   │
│   ├── Helpers/                         # 9 helpers
│   │   ├── JwtHelper.cs
│   │   ├── JwtSettings.cs
│   │   ├── JwtAuthorizeAttribute.cs
│   │   ├── EmailService.cs
│   │   ├── PasswordHasher.cs
│   │   ├── RateLimiter.cs
│   │   ├── HtmlSanitizer.cs
│   │   ├── EntityExtensions.cs
│   │   └── AuthBootstrap.cs
│   │
│   ├── Data/
│   │   └── GiveAIDContext.cs           # EF6 DbContext, 18 DbSets, snake_case convention
│   │
│   ├── Models/
│   │   └── EntityModels.cs             # 20 entity classes
│   │
│   └── docs/                           # Additional documentation
│
├── GiveAID.Client/
│   ├── src/
│   │   ├── App.js                      # Router: public + admin routes, ProtectedRoute
│   │   ├── config.js                   # API endpoints, roles, payment methods
│   │   │
│   │   ├── contexts/
│   │   │   └── AuthContext.js          # React Context for auth state
│   │   │
│   │   ├── services/
│   │   │   ├── api.js                  # Axios instance + interceptors
│   │   │   ├── authService.js          # Auth class + singleton export
│   │   │   ├── stripeService.js        # Stripe.js wrapper
│   │   │   ├── statisticsService.js    # Stats API calls
│   │   │   └── index.js                # Aggregated service exports
│   │   │
│   │   ├── pages/                      # 49 pages
│   │   │   ├── HomePage.js
│   │   │   ├── CampaignDetailPage.js
│   │   │   ├── DonatePage.js
│   │   │   ├── DashboardPage.js
│   │   │   ├── MyDonationsPage.js
│   │   │   ├── HelpCentrePage.js
│   │   │   ├── LoginPage.js
│   │   │   ├── RegisterPage.js
│   │   │   └── admin/
│   │   │       ├── AdminDashboard.js
│   │   │       ├── AdminCampaignPage.js
│   │   │       ├── AdminDonationsPage.js
│   │   │       ├── AdminUsersPage.js
│   │   │       ├── AdminEmailLogsPage.js
│   │   │       └── ... (10 more)
│   │   │
│   │   ├── components/
│   │   │   ├── Navbar.js
│   │   │   ├── Footer.js
│   │   │   ├── ProtectedRoute.js
│   │   │   └── AuthBootstrap.js
│   │   │
│   │   ├── layouts/
│   │   │   └── AdminLayout.js
│   │   │
│   │   └── styles/
│   │       ├── App.css
│   │       ├── AboutPages.css
│   │       └── ... (page-specific CSS files)
│   │
│   └── public/
│       └── assets/                      # Static assets
│
├── GiveAID.Tests/                       # Unit test project (no tests visible)
│
├── database/
│   ├── migrations/                     # SQL migration scripts
│   │   ├── NGO_Database_Gallery_Complete_Migration.sql
│   │   ├── NGO_Database_Gallery_Vietnamese_Migration_V2.sql
│   │   ├── NGO_Database_PaymentGateway_Migration.sql
│   │   ├── NGO_Database_EmailLogs_Migration.sql
│   │   └── ...
│   └── archive/                        # Old migration files
│
├── .github/                            # GitHub workflows
│
├── ARCHITECTURE_ANALYSIS.md            # This file
├── AUDIT_REPORT.md
├── SETUP_GUIDE.md
├── SETUP_GUIDE.docx
└── SMTP_SETUP.md
```
