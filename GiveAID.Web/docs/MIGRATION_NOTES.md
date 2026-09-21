# Programmes to Campaigns Migration Notes

## Overview

The `Programme` entity and its related tables have been **merged into the `Campaign` entity**. The unified `Campaign` model now supports both donation-based campaigns and event/activity-style programmes (the legacy `Programme` behaviour).

This migration removes the now-unused code paths while **preserving all live data** in the `Programmes`, `ProgrammeRegistrations`, and `ProgrammePhotos` database tables.

---

## What Was Merged

The `Campaign` model already absorbs the following fields from `Programme`:

| Former `Programme` field | Now on `Campaign` | Notes |
|---|---|---|
| `Title` | `CampaignName` | Renamed |
| `ProgrammeType` | `ProgrammeType` | e.g. Education, Healthcare, Nutrition |
| `RegistrationRequired` | `RegistrationRequired` | Controls event registration vs. donation-only |
| `MaxParticipants` | `MaxParticipants` | Capacity cap for registration-based events |
| `TargetBeneficiaries` | `TargetBeneficiaries` | Non-monetary impact tracking |
| `ExpectedBudget` / `ActualBudget` | `ExpectedBudget` / `ActualBudget` | Preserved for accounting parity |
| `StartDate` / `EndDate` | `StartDate` / `EndDate` | Already existed on Campaign |

Registration data follows the same pattern: `ProgrammeRegistration` records → `CampaignRegistration` records. Both are tracked separately in the DB but share the same user-facing UI (the unified `/my-registrations` page).

---

## Backend Changes

### Files Deleted

- `GiveAID.Web/Controllers/ProgrammesController.cs` — removed (all endpoints now served by `CampaignsController`)

### Entities Removed from EF

- `GiveAID.Web/Models/EntityModels.cs`:
  - `Programme` class (was `[Table("Programmes")]`)
  - `ProgrammePhoto` class (was `[Table("ProgrammePhotos")]`)
  - `ProgrammeRegistration` class (was `[Table("ProgrammeRegistrations")]`)
  - `Gallery.ProgrammeId` FK property — removed (column stays in DB, no EF FK)

- `GiveAID.Web/Data/GiveAIDContext.cs`:
  - `public DbSet<Programme> Programmes` — removed
  - `public DbSet<ProgrammePhoto> ProgrammePhotos` — removed
  - `public DbSet<ProgrammeRegistration> ProgrammeRegistrations` — removed
  - Fluent API configs for `Programme` and `ProgrammeRegistration` — removed

### Controllers Updated

- **`GalleryController.cs`**: Programme FK lookups (`GetProgrammes`, `GetById`) now use raw SQL against the `Programmes` table directly. No EF dependency on `Programme` entity. The `/api/gallery/programmes` endpoint is marked `[Obsolete]`.
- **`AdminDashboardController.cs`**: `activeProgrammes` and `programmeRegistrations` stats now use raw SQL against the legacy tables. Counts gracefully fall back to 0 if the tables are later dropped.

---

## Frontend Changes

### Files Deleted

- `GiveAID.Client/src/pages/ProgrammeDetailPage.js`
- `GiveAID.Client/src/pages/ProgrammeDetailPage.css`
- `GiveAID.Client/src/pages/ProgrammesPage.js`

### Files Updated

- **`App.js`**: Removed `/programmes` and `/programmes/:id` routes and their imports.
- **`services/index.js`**: Removed `programmesService` export and `programmes` from the default `services` object.
- **`config.js`**: Removed `PROGRAMMES` legacy alias endpoints and `PROGRAMME_TYPES` / `PROGRAMME_STATUS` constants.
- **`components/Footer.js`**: "Programmes" footer link now points to `/campaigns`.

### What Still Uses `programmeType` (Campaign field)

The `Campaign` entity retains `programmeType` as a campaign classification field. It is used in:
- `CampaignDetailPage.js` — displays programme type badge
- `CampaignsPage.js` — filter by programme type
- `MyRegistrationsPage.js` — display programme type in registration cards
- `CampaignsController.cs` — `eventsOnly` filter uses `programmeType IS NOT NULL`

---

## Database Status

### Existing Data (NOT deleted)

The following tables **still contain live data** and are **NOT dropped** by this PR:

- `dbo.Programmes` — 14 seeded programmes
- `dbo.ProgrammeRegistrations` — 40+ seeded registrations
- `dbo.ProgrammePhotos` — gallery photos linked to programmes
- `dbo.Campaigns` — now also stores events (via merged `programmeType` / `RegistrationRequired` fields)

The migration script `001_add_campaign_merged_fields.sql` (in `GiveAID.Web/Database/Migrations/`) only **added columns to the `Campaigns` table** — it did NOT migrate data from the `Programmes` table. Data from both tables continues to exist independently.

### Database Cleanup Script

To drop the legacy Programme tables **after** confirming the migration is stable:

```
database/migrations/NGO_Database_Remove_Programmes_Migration.sql
```

This script:
1. Archives row counts for record-keeping
2. Drops FK constraints referencing `Programmes`
3. Drops `ProgrammePhotos`, `ProgrammeRegistrations`, `Programmes` in correct dependency order
4. Leaves `Gallery.programme_id` column (nullable, may contain orphaned IDs)

**⚠️ Only run this after**:
1. The application has been running in production with no issues
2. You have reviewed the archived row counts
3. No backward compatibility with legacy `/api/programmes` is needed

---

## API Endpoint Mapping

| Old Endpoint | New Endpoint | Status |
|---|---|---|
| `GET /api/programmes` | `GET /api/campaigns?eventsOnly=true` | Replaced |
| `GET /api/programmes/{id}` | `GET /api/campaigns/{id}` | Replaced |
| `POST /api/programmes/{id}/register` | `POST /api/campaigns/{id}/register` | Replaced |
| `GET /api/programmes/my-registrations` | `GET /api/campaigns/my-registrations` | Replaced |
| `GET /api/programmes/{id}/registrations` | `GET /api/campaigns/{id}/registrations` | Replaced |
| `POST /api/programmes` | `POST /api/campaigns` | Replaced |
| `PUT /api/programmes/{id}` | `PUT /api/campaigns/{id}` | Replaced |
| `GET /api/gallery/programmes` | (deprecated) | Still works via raw SQL; returns legacy data |

---

## Public URLs

| Old URL | New URL | Status |
|---|---|---|
| `/programmes` | `/campaigns` | Redirects via footer/navbar update |
| `/programmes/:id` | `/campaigns/:id` | Route removed; 404 |

---

## Rollback

To revert this migration:

1. Restore `ProgrammesController.cs` from git history
2. Restore the three entity classes in `EntityModels.cs`
3. Restore the `DbSet<>` entries and fluent API configs in `GiveAIDContext.cs`
4. Restore the deleted frontend files
5. Do NOT run the `NGO_Database_Remove_Programmes_Migration.sql` script

The database tables are untouched by this PR — no data loss occurs unless the cleanup migration is explicitly run.
