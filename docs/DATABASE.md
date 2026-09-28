# Database — GiveAID v2.0

## 1. Schema Overview

SQL Server 2019+ (Express edition supported). Schema managed by **EF Core 8 migrations**
in `src/Infrastructure/Persistence/Migrations/`.

### ER Diagram (textual)

```
┌──────────────┐         ┌──────────────┐         ┌──────────────────┐
│ Users        │────┐    │ Causes       │────┐    │ Campaigns        │
│              │    │    │              │    │    │                  │
│ UserId (PK)  │    │    │ CauseId (PK) │◀───┼────│ CauseId (FK)     │
│ Email        │    │    │ Name         │    │    │ CampaignId (PK)  │
│ PasswordHash │    │    │ ParentCauseId│──┐ │    │ Name             │
│ Role         │    │    │ Code         │  │ │    │ Goal             │
│ FullName     │    │    └──────────────┘  │ │    │ Raised           │
│ IsActive     │    │                       │ │    │ Status           │
└──────────────┘    │                       │ │    └────────┬─────────┘
       │            │                       │ │             │
       │ 1          │                       │ │             │ 1
       │            └───────────────────────┘ │             │
       │                                      │             │
       │ *                                    │ *           │ *
┌──────▼─────────────┐         ┌──────────────▼──────┐  ┌───▼───────────────┐
│ Donations          │         │ CampaignRegistrations│  │ CampaignReports   │
│                    │         │                      │  │                   │
│ DonationId (PK)    │         │ RegId (PK)           │  │ ReportId (PK)     │
│ UserId (FK)        │         │ CampaignId (FK)      │  │ CampaignId (FK)   │
│ CampaignId (FK)    │         │ UserId (FK)          │  │ Title             │
│ Amount             │         │ RegisteredAt         │  │ Content           │
│ Currency           │         │ Status               │  │ PublishedAt       │
│ Status             │         └──────────────────────┘  └───────────────────┘
│ PaymentMethod      │
│ TransactionId      │
│ StripeIntentId     │
└────────────────────┘
```

(Other entities — Gallery, Team, Achievements, Careers, FAQs, Contact, Conversations,
Invitations, CMS, EmailLogs, WebhookLogs — follow the same pattern.)

## 2. Tables

### 2.1. `Users`

| Column        | Type            | Null | Notes                                    |
|---------------|-----------------|------|------------------------------------------|
| UserId        | int             | No   | PK, identity                             |
| Email         | nvarchar(256)   | No   | Unique index                             |
| Username      | nvarchar(64)    | Yes  | Optional display name                    |
| PasswordHash  | nvarchar(512)   | No   | PBKDF2 SHA-256 base64                    |
| Role     | nvarchar(32)    | No   | `User`, `Admin`, `ContentManager`      |
| FullName      | nvarchar(128)   | Yes  |                                          |
| IsActive      | bit             | No   | Default 1                                |
| IsLocked      | bit             | No   | Default 0                                |
| FailedLoginCount | int          | No   | Default 0                                |
| LockoutEnd    | datetime2       | Yes  |                                          |
| LastLoginAt   | datetime2       | Yes  |                                          |
| CreatedAt     | datetime2       | No   | Default `SYSUTCDATETIME()`               |
| UpdatedAt     | datetime2       | Yes  |                                          |

Indexes: `Email` (unique), `Role`.

### 2.2. `Causes`

| Column        | Type            | Null | Notes                                  |
|---------------|-----------------|------|----------------------------------------|
| CauseId       | int             | No   | PK                                     |
| Name          | nvarchar(128)   | No   |                                        |
| Code          | nvarchar(16)    | No   | `CHILD`, `EDU`, `DIS`, etc.            |
| Description   | nvarchar(2000)  | Yes  |                                        |
| ParentCauseId | int             | Yes  | Self-FK for hierarchy                  |
| IsActive      | bit             | No   | Default 1                              |
| DisplayOrder  | int             | No   | Default 0                              |
| CreatedAt     | datetime2       | No   |                                        |

Indexes: `Code` (unique), `ParentCauseId`.

### 2.3. `Campaigns`

| Column              | Type           | Null | Notes                              |
|---------------------|----------------|------|------------------------------------|
| CampaignId          | int            | No   | PK                                 |
| Name                | nvarchar(200)  | No   |                                    |
| Description         | nvarchar(max)  | Yes  |                                    |
| CauseId             | int            | No   | FK → Causes                        |
| Goal                | decimal(18,2)  | No   |                                    |
| Raised              | decimal(18,2)  | No   | Default 0                          |
| StartDate           | datetime2      | No   |                                    |
| EndDate             | datetime2      | No   |                                    |
| Status              | nvarchar(32)   | No   | `Draft`, `Active`, `Completed`, `Cancelled` |
| Featured            | bit            | No   | Default 0                          |
| RegistrationRequired| bit            | No   | Default 0 — true = registration-only |
| ImageUrl            | nvarchar(1024) | Yes  |                                    |
| CreatedAt           | datetime2      | No   |                                    |
| UpdatedAt           | datetime2      | Yes  |                                    |

Indexes: `CauseId`, `Status`, `Featured`.

### 2.4. `Donations`

| Column                | Type           | Null | Notes                              |
|-----------------------|----------------|------|------------------------------------|
| DonationId            | int            | No   | PK                                 |
| UserId                | int            | Yes  | FK → Users (null = anonymous)      |
| CampaignId            | int            | No   | FK → Campaigns                     |
| Amount                | decimal(18,2)  | No   |                                    |
| Currency              | nvarchar(3)    | No   | ISO 4217, default `USD`            |
| PaymentMethod         | nvarchar(32)   | No   | `CreditCard`, `DebitCard`, etc.    |
| Status                | nvarchar(32)   | No   | `Pending`, `Completed`, `Failed`, `Refunded` |
| TransactionId         | nvarchar(64)   | No   | Unique, format `TXN-<guid>`        |
| StripePaymentIntentId | nvarchar(128)  | Yes  |                                    |
| CardLast4             | nvarchar(4)    | Yes  |                                    |
| Anonymous             | bit            | No   | Default 0                          |
| DonorName             | nvarchar(128)  | Yes  | For anonymous donations            |
| DonorEmail            | nvarchar(256)  | Yes  |                                    |
| DonorMessage          | nvarchar(2000) | Yes  |                                    |
| CreatedAt             | datetime2      | No   |                                    |
| CompletedAt           | datetime2      | Yes  |                                    |

Indexes: `UserId`, `CampaignId`, `Status`, `TransactionId` (unique).

### 2.5. `Galleries`

| Column        | Type           | Null | Notes                                |
|---------------|----------------|------|--------------------------------------|
| GalleryId     | int            | No   | PK                                   |
| Title         | nvarchar(200)  | No   |                                      |
| Description   | nvarchar(2000) | Yes  |                                      |
| ImageUrl      | nvarchar(1024) | No   |                                      |
| ThumbnailUrl  | nvarchar(1024) | Yes  |                                      |
| Category      | nvarchar(64)   | Yes  |                                      |
| ProgrammeId   | int            | Yes  | Legacy FK                             |
| CauseId       | int            | Yes  |                                      |
| DisplayOrder  | int            | No   | Default 0                            |
| IsActive      | bit            | No   | Default 1                            |
| CreatedAt     | datetime2      | No   |                                      |

### 2.6. `Conversations` and `ConversationMessages`

Two-table design: `Conversations` is the thread, `ConversationMessages` is each post.

`Conversations`:
| Column              | Type          | Null | Notes                                  |
|---------------------|---------------|------|----------------------------------------|
| ConversationId      | int           | No   | PK                                     |
| UserId              | int           | No   | FK → Users                             |
| AssignedAdminId     | int           | Yes  | FK → Users                             |
| Subject             | nvarchar(200) | No   |                                        |
| Status              | nvarchar(32)  | No   | `Open`, `Closed`, `Pending`            |
| LastMessageAt       | datetime2     | Yes  |                                        |
| CreatedAt           | datetime2     | No   |                                        |

`ConversationMessages`:
| Column        | Type           | Null | Notes                          |
|---------------|----------------|------|--------------------------------|
| MessageId     | int            | No   | PK                             |
| ConversationId| int            | No   | FK → Conversations             |
| SenderId      | int            | No   | FK → Users                     |
| SenderRole    | nvarchar(16)   | No   | `User`, `Admin`                |
| Body          | nvarchar(max)  | No   |                                |
| ReadAt        | datetime2      | Yes  |                                |
| CreatedAt     | datetime2      | No   |                                |

### 2.7. `EmailLogs`

| Column        | Type           | Null | Notes                              |
|---------------|----------------|------|------------------------------------|
| EmailLogId    | bigint         | No   | PK, identity                       |
| To            | nvarchar(256)  | No   |                                    |
| From          | nvarchar(256)  | No   |                                    |
| Subject       | nvarchar(512)  | No   |                                    |
| Body          | nvarchar(max)  | Yes  |                                    |
| TemplateKey   | nvarchar(64)   | Yes  |                                    |
| Status        | nvarchar(32)   | No   | `Queued`, `Sent`, `Failed`, `Retrying` |
| ErrorMessage  | nvarchar(2000) | Yes  |                                    |
| RetryCount    | int            | No   | Default 0                          |
| SentAt        | datetime2      | Yes  |                                    |
| CreatedAt     | datetime2      | No   |                                    |

Indexes: `Status`, `CreatedAt`.

### 2.8. `WebhookLogs`

| Column        | Type           | Null | Notes                              |
|---------------|----------------|------|------------------------------------|
| WebhookLogId  | bigint         | No   | PK                                 |
| Source        | nvarchar(32)   | No   | `Stripe`, `PayPal`, etc.           |
| EventType     | nvarchar(64)   | No   |                                    |
| EventId       | nvarchar(128)  | Yes  | Idempotency key                    |
| Payload       | nvarchar(max)  | No   | Raw JSON                           |
| Signature     | nvarchar(512)  | Yes  | For verification                   |
| Status        | nvarchar(32)   | No   | `Received`, `Processed`, `Failed`  |
| ErrorMessage  | nvarchar(2000) | Yes  |                                    |
| ProcessedAt   | datetime2      | Yes  |                                    |
| CreatedAt     | datetime2      | No   |                                    |

Indexes: `Source`, `EventType`, `EventId` (unique within source).

## 3. Migration Order

EF Core generates timestamped migrations. For a fresh database:

```powershell
# Apply all pending migrations
dotnet ef database update --project src/Infrastructure/GiveAID.V2.Infrastructure.csproj --startup-project src/WebApi/GiveAID.V2.WebApi.csproj
```

The first run also runs `DatabaseSeeder` which inserts:
- 1 `Admin` user (`admin@give-aid.org` / `Admin@123`)
- 5 demo causes (`CHILD`, `EDU`, `DIS`, `WOMAN`, `YOUTH`)
- 3 demo FAQs

For the **legacy v1 → v2** migration path, see [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md)
and `database/migrations/`.

## 4. Indexes Cheat Sheet

| Table                    | Indexes                                                |
|--------------------------|--------------------------------------------------------|
| Users                    | `Email` UNIQUE, `Role`                                 |
| Causes                   | `Code` UNIQUE, `ParentCauseId`                         |
| Campaigns                | `CauseId`, `Status`, `Featured`                        |
| Donations                | `UserId`, `CampaignId`, `Status`, `TransactionId` UNIQUE |
| Conversations            | `UserId`, `AssignedAdminId`, `Status`, `LastMessageAt` |
| ConversationMessages     | `ConversationId`, `SenderId`                           |
| ContactMessages          | `Status`, `CreatedAt`                                  |
| Invitations              | `ReferrerId`, `Status`, `Token` UNIQUE                 |
| EmailLogs                | `Status`, `CreatedAt`                                  |
| WebhookLogs              | `Source`, `EventType`, `EventId` UNIQUE                |
| Galleries                | `Category`, `CauseId`, `ProgrammeId`                   |

## 5. Connection String

### Local development (single source of truth)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=GiveAIDDB;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True;Connect Timeout=15"
  }
}
```

LocalDB ships with Visual Studio — no separate SQL Server install is required.
To switch to `.\SQLEXPRESS` instead, see [DEPLOYMENT.md](DEPLOYMENT.md).

### Staging / Production

Never commit a real connection string. Set it via the environment variable:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=prod-sql;Database=GiveAIDDB;User Id=app;Password=...;Encrypt=True;TrustServerCertificate=False"
```

For Azure SQL, Managed Identity, or AWS RDS use the connection strings
documented in [DEPLOYMENT.md](DEPLOYMENT.md) section 3.

## 6. Backup & Recovery

| Schedule     | Type        | Retention |
|--------------|-------------|-----------|
| Daily 02:00  | Full        | 30 days   |
| Hourly       | Differential | 7 days    |
| Weekly       | Full + offsite | 1 year |

Restore command:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "RESTORE DATABASE [GiveAIDDB] FROM DISK='D:\Backups\GiveAIDDB_Full.bak' WITH NORECOVERY, REPLACE"
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "RESTORE DATABASE [GiveAIDDB] FROM DISK='D:\Backups\GiveAIDDB_Diff.bak' WITH RECOVERY"
```

## 7. Seeding

`src/Infrastructure/Persistence/Seed/DatabaseSeeder.cs` runs on app start (idempotent):

```csharp
public async Task SeedAsync()
{
    if (!await _context.Users.AnyAsync(u => u.Role == "Admin"))
    {
        _context.Users.Add(new User
        {
            Email = "admin@give-aid.org",
            PasswordHash = _hasher.Hash("Admin@123"),
            Role = "Admin",
            IsActive = true
        });
        await _context.SaveChangesAsync();
    }
}
```

## 8. Performance Notes

- All FK columns are indexed
- All `Status` enum columns are indexed (used in every list query)
- `Donations.TransactionId` is unique-indexed for idempotency
- `EmailLogs.CreatedAt` is indexed for retention queries
- Hot reads (statistics, causes list) are cached via `MemoryCacheService`
- Use `AsNoTracking()` in query handlers for read-only paths

## 9. Data Retention

| Table         | Retention                          |
|---------------|------------------------------------|
| EmailLogs     | 90 days, then soft-archive         |
| WebhookLogs   | 30 days, then hard-delete          |
| ContactMessages | 1 year                            |
| Conversations | Indefinite (user data)             |
| Donations     | Indefinite (financial record)      |
| EmailLogs (PII) | Anonymise after 90 days          |

Run nightly via SQL Agent job — see `database/migrations/retention.sql`.
