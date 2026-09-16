# Database

> Audience: AI coding assistants. The DB schema is owned by SQL scripts in the project root.
> Entity Framework is read-only (`Database.SetInitializer(null)`) — it never modifies the schema.

---

## 1. Connection

`Web.config` → `connectionStrings["GiveAIDContext"]`:

```
Data Source=.\SQLEXPRESS; Initial Catalog=GiveAIDDB; Integrated Security=True
```

To change the connection, edit `Web.config` only. Do not move it to code.

---

## 2. Migration order (run once on a fresh DB)

The `*.sql` scripts in the project root are the canonical schema source. Apply them in this order:

| # | Script | Purpose | Idempotent? |
|---|---|---|---|
| 1 | `NGO_Database_Causes_Restructure_Migration.sql` | Adds `parent_cause_id` to `Causes`; restructures to 9 parent causes + 27 sub-causes | ⚠️  Run once on dev DB only |
| 2 | `NGO_Database_CampaignProgramme_Merge.sql` | Adds `programme_type`, `registration_required`, etc. to `Campaigns`; backfills from `Programmes` | ⚠️  Run once |
| 3 | `NGO_Database_Invitations_Migration.sql` | Creates `Invitations` table | ✅  IF NOT EXISTS |
| 4 | `NGO_Database_Invitations_PascalCase_Patch.sql` | Renames PascalCase columns to snake_case if needed | ✅  Guards on column existence |
| 5 | `DB_Patch_Combined.sql` | Master patch: Invitations rename + Gallery FK drop + donation_date DEFAULT | ✅  All steps guarded |
| 6 | `Donation_DonationDate_Default_Patch.sql` | Adds DEFAULT GETDATE() on `Donations.donation_date` (subsumed by step 5) | ✅ |
| 7 | `Campaigns_DataSeed.sql` | Seeds 8 realistic campaigns + 13 donations using `MERGE` | ✅  Idempotent |
| 8 | `Campaigns_Fixup.sql` | Cleans up placeholder campaigns + inserts Warm Winter (dev only) | ⚠️  Destructive |

**Combined one-shot** for an existing dev DB:

```sql
USE GiveAIDDB;
GO

-- Step A — structural
DB_Patch_Combined.sql

-- Step B — causes + campaign/programme merge
NGO_Database_Causes_Restructure_Migration.sql
NGO_Database_CampaignProgramme_Merge.sql

-- Step C — invitations
NGO_Database_Invitations_Migration.sql

-- Step D — data
Campaigns_DataSeed.sql
```

---

## 3. Tables (high-level)

| Table | Purpose | Key columns |
|---|---|---|
| `Users` | All user accounts (admin + regular) | `email` UNIQUE, `role`, `is_active`, `password_hash` |
| `Causes` | 2-level cause taxonomy | `cause_code`, `parent_cause_id` (nullable, self-FK), `is_parent_cause` |
| `Organizations` | Partner NGOs | `name`, `is_active` |
| `Campaigns` | Donation + event campaigns | `cause_id`, `campaign_code` UNIQUE, `goal_amount`, `raised_amount`, `status`, `programme_type` |
| `CampaignReports` | Per-campaign financial reports | `campaign_id`, `total_received`, `total_spent` |
| `Donations` | Donation records | `user_id`, `campaign_id`, `cause_id`, `amount`, `payment_method`, `payment_status`, `transaction_id` |
| `CampaignRegistrations` | User event registrations | `(campaign_id, user_id)` UNIQUE |
| `Conversations` | User-to-admin queries | `user_id`, `subject`, `status` |
| `ConversationMessages` | Messages in a conversation | `conversation_id`, `sender_id` |
| `CmsPages` | Editable CMS content blocks | `slug`, `title`, `content` |
| `Careers` | Job postings | `slug`, `is_active`, `application_deadline` |
| `CareerApplications` | Job applications | `career_id`, `user_id` |
| `Gallery` | Photo gallery items | `programme_id` (nullable, FK dropped), `campaign_id` (optional) |
| `ContactMessages` | Contact form submissions | `email`, `subject`, `status` |
| `TeamMembers` | About-us team profiles | `is_active`, `display_order` |
| `Achievements` | Impact stats (numeric) | `metric_value`, `featured` |
| `Faqs` | FAQ entries | `is_active`, `display_order`, `category` |
| `Invitations` | Referral invitations | `invitation_token`, `invitee_email`, `status` |
| `Programmes` *(legacy)* | Read-only via `ProgrammesController` | kept for Gallery FK |
| `ProgrammePhotos` *(legacy)* | — | — |
| `ProgrammeRegistrations` *(legacy)* | — | — |

Full entity definitions are in `GiveAID.Web/Models/EntityModels.cs`.

---

## 4. Cause hierarchy (current)

The migration script seeds **9 parent causes** + **27 sub-causes**:

```
EDU      → Education for children         (5 sub-causes)
NUTRI    → Nutrition & food               (3 sub-causes)
HEALTH   → Medical care                   (3 sub-causes)
WATER    → Clean water & sanitation        (2 sub-causes)
SPECIAL  → Special circumstances          (4 sub-causes)
CLOTH    → Clothing & essentials          (2 sub-causes)
PROTECT  → Child protection               (2 sub-causes)
EMERG    → Emergency relief               (3 sub-causes)
FUTURE   → Future development             (3 sub-causes)
```

Each parent has `is_parent_cause = 1`; sub-causes have `parent_cause_id` set to the parent.
The `CausesController.GetTree()` endpoint returns this hierarchy as a nested JSON for the
frontend `CausesPage`.

---

## 5. Naming conventions

- **Tables & columns:** `snake_case` (`user_id`, `campaign_id`, `goal_amount`)
- **C# entity properties:** `PascalCase` (`UserId`, `CampaignId`, `GoalAmount`)
- **JSON DTOs:** `camelCase` (`userId`, `campaignId`, `goalAmount`)
- **Money columns:** `DECIMAL(18,2)` — never `FLOAT` or `MONEY`

The mapping C# → SQL is automatic via `SnakeCaseColumnNameConvention` in
`GiveAIDContext.OnModelCreating()`. Don't add manual `.HasColumnName(...)` calls unless you
absolutely need to break the convention.

---

## 6. Common SQL pitfalls (and how we avoided them)

| Pitfall | How we handled it |
|---|---|
| `donation_date` defaulting to `0001-01-01` (C# min value) | `DB_Patch_Combined.sql` adds `DEFAULT GETDATE()` + backfills any bad rows |
| `Invitations` columns in PascalCase (legacy migration) | `DB_Patch_Combined.sql` renames them via `sp_rename` + rebuilds indexes |
| `Gallery.programme_id` FK prevents deletion of orphan Programmes | `DB_Patch_Combined.sql` drops the FK + makes column nullable |
| Orphan `Campaigns.cause_id` after causes restructure | `NGO_Database_CampaignProgramme_Merge.sql` adds CHECK `CHK_Campaigns_CauseExists` (guard) |
| Hardcoded seed campaign cause IDs breaking after restructure | `Campaigns_DataSeed.sql` resolves by `cause_code`, not by ID |

---

## 7. Soft delete vs hard delete

| Entity | Strategy | Field |
|---|---|---|
| `Causes` | Soft delete (sets `is_active = 0`) | `IsActive` |
| `Campaigns` | Hard delete + FK cascade to Donations | — |
| `Gallery` | Hard delete | — |
| `Users` | Soft delete (set `is_active = 0`) | `IsActive` |
| `Achievements`, `Faqs`, `TeamMembers` | Soft delete via `is_active` | `IsActive` |
| `Donations` | Never deleted (audit trail) | — |
| `ContactMessages` | Soft-mark via `status` | `Status` |

---

## 8. JSON contract (for the API layer)

Successful response:

```json
{
  "success": true,
  "message": "OK",
  "data": { /* entity or list */ }
}
```

Failed response (single error):

```json
{
  "success": false,
  "message": "Campaign not found.",
  "data": null
}
```

Validation failure:

```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": {
    "Email": ["Email is required.", "Email is not valid."],
    "Password": ["Password must be at least 8 characters."]
  }
}
```

Frontend services assume this shape; if you change it, update `services/api.js` and every
`response.success`/`response.data` consumer.

---

## 9. Money handling

- DB stores `DECIMAL(18,2)` to avoid float drift
- C# properties are `decimal`
- JSON serialises as a **string** to preserve precision across the wire (e.g. `"21500000.00"`)
- Frontend formats with `Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })`

If you add a new money field, follow the same pattern or the UI will show `21,500,000.00 ₫`
incorrectly.

---

## 10. Resetting the DB (development only)

```sql
USE master;
GO
ALTER DATABASE GiveAIDDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO
DROP DATABASE GiveAIDDB;
GO
CREATE DATABASE GiveAIDDB;
GO
USE GiveAIDDB;
GO

-- Re-run migrations in order
:r C:\path\to\DB_Patch_Combined.sql
:r C:\path\to\NGO_Database_Causes_Restructure_Migration.sql
:r C:\path\to\NGO_Database_CampaignProgramme_Merge.sql
:r C:\path\to\NGO_Database_Invitations_Migration.sql
:r C:\path\to\Campaigns_DataSeed.sql
```

After reset, restart the backend — `GiveAIDContext.SeedDatabase()` will recreate the admin +
demo users.
