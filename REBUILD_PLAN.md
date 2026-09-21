# GiveAID v2.0 — Rebuild Strategy Plan
**Date:** 2026-09-19  
**Phase:** Strategic Rebuild Plan  

---

## Phased Approach

The rebuild is organized into 3 phases. Phase A fixes the blockers. Phase B restores missing features. Phase C adds polish and real integrations.

---

## Phase A — Critical Blockers (Week 1, Est. 4–6 hours)

### A1. Fix JWT Role Case Mismatch — Unblock All Admin Routes
**Priority:** CRITICAL — unblocks all 19 admin-gated endpoints

**Root Cause:** `JwtAuthorizeAttribute.cs:38` does case-sensitive string comparison against JWT role claims that are lowercase.

**Fix — Option 1 (Recommended):** Normalize case in `JwtAuthorizeAttribute.cs`:
```csharp
// JwtAuthorizeAttribute.cs line ~38
var allowed = Roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(r => r.Trim());
var userRole = principal.FindFirst(ClaimTypes.Role)?.Value;
if (userRole == null || !allowed.Contains(userRole, StringComparer.OrdinalIgnoreCase))
{
    return false;
}
```

**Fix — Option 2:** Normalize in `JwtHelper.GenerateToken()`:
```csharp
// JwtHelper.cs — when creating role claim:
new Claim(ClaimTypes.Role, user.Role, ClaimValueTypes.String, JwtSettings.Issuer),
// Add .ToLowerInvariant() or ensure DB roles are consistent
```

**Files:** `GiveAID.Web\Helpers\JwtAuthorizeAttribute.cs`

**Est. Effort:** 15 minutes

**Verification:** After fix, `GET http://localhost:5231/api/v1/admin/stats` with admin JWT should return 200.

---

### A2. Fix Login Field Mismatch — Login by Username
**Priority:** CRITICAL — users cannot log in with username field

**Root Cause:** `LoginRequest` in `AuthController.cs:299` uses `Email` field, but React `LoginPage.js` sends `username`.

**Fix:** Add `Username` to `LoginRequest` and query by both:
```csharp
// AuthController.cs ~line 40
public class LoginRequest {
    public string Email { get; set; }  // keep for backwards compat
    public string Username { get; set; } // ADD
}
```
```csharp
// AuthController.cs ~line 40-45
var identifier = (!string.IsNullOrWhiteSpace(request.Username)
    ? request.Username
    : request.Email);
var user = _context.Users.FirstOrDefault(u =>
    u.Username == identifier || u.Email == identifier);
```

**Files:** `GiveAID.Web\Controllers\AuthController.cs`

**Est. Effort:** 30 minutes

---

### A3. Fix DB Schema Mismatches — EF Table Names
**Priority:** HIGH — CMS and Team admin editing fails

**Root Cause:** EF `[Table]` attributes use PascalCase but SQL migrations created snake_case tables.

**Affected Files:**
- `GiveAID.Web\Models\EntityModels.cs:540` — `[Table("CmsPages")]` → `[Table("cms_pages")]`
- `GiveAID.Web\Models\EntityModels.cs:795` — `[Table("TeamMembers")]` → `[Table("team_members")]`

**Est. Effort:** 15 minutes per table, 30 minutes total

---

## Phase B — Restore Missing Features (Week 1–2, Est. 8–12 hours)

### B1. Seed All Empty Tables
**Priority:** HIGH — 4 content tables are completely empty

**Tables to seed:**
1. `Achievements` — 0 rows (About/Achievements page empty)
2. `TeamMembers` — 0 rows (About/Team page empty)
3. `Careers` — 0 rows (Careers page empty)
4. `Donations` — 0 rows (My Donations empty)

**Approach:** Write seed SQL migration scripts (like existing `Campaigns_DataSeed.sql`) for each table. Alternatively, expose admin CRUD UI for each after fixing Phase A.

**Commands:**
```sql
-- Example: Seed Achievements
INSERT INTO achievements (title, category, description, metric_value, metric_label, metric_suffix,
  is_active, display_order, created_at)
VALUES
  ('10,000 Children Reached', 'Impact', 'Milestone of 10,000 children helped across Vietnam', 10000,
   'Children Supported', '+', 1, 1, GETUTCDATE()),
  ...;
```

**Est. Effort:** 2–3 hours per table × 4 tables = 8–12 hours

---

### B2. Align React config.js with Actual API Routes
**Priority:** HIGH — prevents silent API failures

**Problem:** `config.js` references 4 non-existent endpoint patterns:
- `/api/v1/achievements/stats` → Controller has no `/stats` route
- `/api/v1/causes/stats` → Controller has no `/stats` route
- `/api/v1/gallery/programmes` → Controller has no `/programmes` route
- `/api/v1/statistics/dashboard` → Controller uses `Route("overview")` not `"dashboard"`

**Fix Options:**
1. **Option A (Recommended):** Add missing routes to controllers (adds features)
2. **Option B:** Update `config.js` to match actual routes (quick cleanup)

**Option A — Add routes to controllers:**

```csharp
// AchievementsController.cs — add after existing endpoints:
[HttpGet]
[Route("stats")]
public IHttpActionResult GetStats() {
    var stats = new {
        total = _context.Achievements.Count(a => a.IsActive),
        featured = _context.Achievements.Count(a => a.IsFeatured && a.IsActive)
    };
    return Ok(new { success = true, data = stats });
}
```

**Est. Effort:** 1–2 hours

---

### B3. Fix Contact Form Public Submission
**Priority:** HIGH — contact form currently requires auth

**Root Cause:** `ContactsController` class-level `[JwtAuthorize]` blocks even the public `POST /contacts` endpoint.

**Fix — Option 1 (Recommended):** Move `POST /contacts` to separate public controller or add `[AllowAnonymous]`:
```csharp
// ContactsController.cs
[HttpPost]
[Route("")]
[AllowAnonymous]  // ADD
public IHttpActionResult Submit([FromBody] ContactSubmissionRequest request) {
    // ...
}
```

**Est. Effort:** 15 minutes

---

### B4. Fix `auth/me` Incomplete Response
**Priority:** MEDIUM — React AuthContext uses this for user state

**Root Cause:** `AuthController.cs:100-110` returns only `{userId, email}`.

**Fix:** Add `username`, `fullName`, `role` fields to response.

**Est. Effort:** 15 minutes

---

### B5. Admin Console Functional Testing
After Phase A fixes, test all admin pages:
- `/admin` — Dashboard
- `/admin/campaigns` — CRUD campaigns
- `/admin/donations` — View all donations
- `/admin/users` — User management
- `/admin/gallery` — Gallery management
- `/admin/achievements` — Achievement management
- `/admin/team` — Team management
- `/admin/cms` — CMS page management
- `/admin/queries` — User query inbox
- `/admin/contacts` — Contact messages
- `/admin/invitations` — Invitation management
- `/admin/emails` — Email log viewer

**Est. Effort:** 2–3 hours testing + bug fixes

---

## Phase C — Production Hardening (Week 2–3, Est. 16–24 hours)

### C1. Real Stripe Payment Integration
**Priority:** HIGH — donations currently use mock payment

**Current State:** `DonatePage.js` imports `loadStripe` but Stripe keys not configured. `GiveAID.Web/Services/Payments/` contains stub implementations.

**Replacement Library:**
- **Frontend:** `@stripe/react-stripe-js` + `@stripe/stripe-js`
- **Backend:** `Stripe.net` NuGet package
- Install: `Install-Package Stripe.net` in GiveAID.Web

**Real Integration Steps:**
1. Get Stripe test keys from https://dashboard.stripe.com/test/apikeys
2. Add to `Web.config`:
```xml
<add key="StripePublishableKey" value="pk_test_..." />
<add key="StripeSecretKey" value="sk_test_..." />
<add key="StripeWebhookSecret" value="whsec_..." />
```
3. Replace mock payment service with real `StripeService.cs`
4. Wire webhook endpoint in `AdminPaymentsController.cs`
5. Add Stripe.js to React `index.html`

**Est. Effort:** 8–12 hours

---

### C2. Email Service Production Configuration
**Priority:** HIGH — email is disabled (`SmtpEnabled=false`)

**Current State:** `EmailService.cs` logs instead of sending. `SmtpEnabled=false` in Web.config.

**Options:**
1. **SMTP relay** (simplest): SendGrid, Mailgun, Amazon SES
2. **SendGrid:** `Install-Package SendGrid`
3. **Configuration:**
```xml
<add key="SmtpEnabled" value="true" />
<add key="SmtpHost" value="smtp.sendgrid.net" />
<add key="SmtpPort" value="587" />
<add key="SmtpUseSsl" value="true" />
<add key="SmtpUsername" value="apikey" />
<add key="SmtpPassword" value="SG.xxxxx" />
```

**Est. Effort:** 2–3 hours

---

### C3. VNPay / MoMo Payment Gateway (Vietnam Market)
**Priority:** MEDIUM — many Vietnamese donors don't use international cards

**For VNPay integration:**
- VNPay provides sandbox at https://sandbox.vnpayment.vn/apis/
- `GiveAID.Web/Services/Payments/VNPayService.cs` (stub exists)
- Frontend redirect flow for VNPay QR/payment

**Est. Effort:** 8–12 hours per gateway

---

### C4. Seed Comprehensive Demo Data
**Priority:** HIGH — platform is empty without real content

**Tables needing seed data:**
- `Achievements` — 5–10 rows with metrics
- `TeamMembers` — 5–8 team members with bios
- `Careers` — 3–5 open positions
- `Donations` — 20–50 sample donation records for statistics
- `CampaignReports` — 2–3 published reports
- `CmsPages` — Privacy Policy, Terms of Service, About Us

**Approach:** Write SQL seed scripts, review with stakeholder, execute.

**Est. Effort:** 4–6 hours

---

### C5. Production Deployment Checklist
**Priority:** HIGH — before going live

1. Set `Environment=Production` in `Web.config`
2. Configure `Cors:AllowedOrigins` for production domain
3. Set `SmtpEnabled=true` with real SMTP credentials
4. Configure `PublicSiteUrl` for email links
5. Set up SQL Server (not LocalDB) for production
6. Configure proper `JwtSecret` (minimum 256-bit)
7. Set up Stripe live keys
8. Enable HTTPS / SSL
9. Set up logging/monitoring (Serilog, Application Insights)
10. Security audit: penetration test, OWASP Top 10 review

**Est. Effort:** 4–8 hours

---

## Library Swap Recommendations

| Component | Current | Recommended Replacement | Reason |
|-----------|---------|------------------------|--------|
| **Admin Charts** | Custom/partial | `recharts` (`npm install recharts`) | Mature, composable React charts |
| **Admin Tables** | Bootstrap tables | `react-bootstrap-table2` (`npm install react-bootstrap-table-next`) | Built-in sorting, pagination, filtering |
| **Admin DatePicker** | browser native | `react-datepicker` (`npm install react-datepicker`) | Consistent cross-browser UI |
| **Admin File Upload** | None | `react-dropzone` (`npm install react-dropzone`) | Drag-and-drop image upload |
| **Admin Rich Text** | None | `@tinymce/tinymce-react` or `react-quill` | CMS content editing |
| **Image Optimization** | Raw `<img>` | `react-lazy-load-image-component` | Performance |
| **Form Validation** | Manual | `react-hook-form` + `yup` | Cleaner forms, schema validation |
| **Loading States** | `Spinner` only | `react-loading-skeleton` | Better perceived performance |
| **Payment UI** | Custom card form | `@stripe/react-stripe-js` Elements | PCI-compliant, production-ready |
| **Email Template** | String concatenation | `Handlebars.NET` | Maintainable email templates |
| **PDF Generation** | None | `QuestPDF` (`Install-Package QuestPDF`) | Clean .NET PDF generation for receipts |

---

## GitHub Open Source Libraries to Integrate

### Admin Console Enhancements
```
npm install @react-admin-alpha/core   # React Admin framework (alternative to raw AdminLTE)
npm install react-admin               # Full-featured admin framework
npm install @dnd-kit/core @dnd-kit/sortable  # Drag-and-drop for gallery ordering
```

### Charts & Analytics
```
npm install recharts                  # Charts for admin dashboard
npm install @nivo/bar @nivo/line      # Advanced analytics visualizations
```

### Payment Gateways
```
npm install @stripe/stripe-js         # Stripe browser SDK
npm install @stripe/react-stripe-js   # React Stripe components
```

### Performance & UX
```
npm install react-lazy-load-image-component  # Lazy load images
npm install react-infinite-scroll-component # Infinite scroll for long lists
npm install react-slick slick-carousel      # Image carousel for gallery
npm install notistack                        # Toast notifications
```

---

## Success Criteria

| Phase | Success Metric |
|-------|---------------|
| Phase A | Admin can log in and access all admin pages — 19/19 endpoints return 200 |
| Phase B | All public pages show content — Achievements, Team, Careers, Donations all have data |
| Phase C | End-to-end donation with real payment works — Stripe webhook fires, receipt sent, DB updated |

---

## Estimated Total Effort

| Phase | Hours | Notes |
|-------|-------|-------|
| Phase A | 4–6h | Mostly single-line fixes |
| Phase B | 8–12h | Seed data + route alignment |
| Phase C | 16–24h | Stripe, email, polish |
| **Total** | **28–42h** | ~2–3 weeks at 4h/day |

---

## Immediate Next Steps (Do Today)

1. **Fix `JwtAuthorizeAttribute.cs:38`** — Add `StringComparer.OrdinalIgnoreCase` to role check → Immediately unblocks admin
2. **Fix `AuthController.cs`** — Add username lookup to login → Immediately fixes user auth
3. **Run seed SQL** for Achievements, Team, Careers → Fills empty pages
4. **Fix `[Table]` attributes** → Unblocks CMS and Team admin editing
5. **Test admin console** end-to-end with browser
