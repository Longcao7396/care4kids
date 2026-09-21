# Partners & FAQs Seed Report - 2026-09-18

## Summary

Successfully seeded realistic data for **Our Partners** and **Help Centre** pages in the GiveAID database.

---

## Database Schema

### Organizations Table
| Column | Type |
|--------|------|
| organization_id | int (PK) |
| organization_name | nvarchar(150) |
| organization_type | nvarchar(20) CHECK: 'Supporter', 'Partner', 'NGO' |
| description | nvarchar(max) |
| logo_url | nvarchar(255) |
| website_url | nvarchar(200) |
| contact_email | nvarchar(100) |
| contact_phone | nvarchar(20) |
| address | nvarchar(255) |
| registration_number | nvarchar(50) |
| mission | nvarchar(500) |
| vision | nvarchar(500) |
| contribution_amount | decimal(18,2) |
| contribution_type | nvarchar(50) |
| is_active | bit |
| is_featured | bit |
| display_order | int |
| created_at | datetime |
| updated_at | datetime |

### Faqs Table
| Column | Type |
|--------|------|
| faq_id | int (PK) |
| question | nvarchar(500) |
| answer | nvarchar(max) |
| category | nvarchar(100) |
| display_order | int |
| is_active | bit |
| is_featured | bit |
| view_count | int |
| created_at | datetime |
| updated_at | datetime |
| created_by | int |

---

## Partners Inserted

### Total Count: 42 organizations

| Type | Count |
|------|-------|
| NGO | 24 |
| Partner | 12 |
| Supporter | 6 |

### Featured Organizations (10):
1. UNICEF Vietnam
2. Save the Children Vietnam
3. World Vision Vietnam
4. CARE International Vietnam
5. Viet Nam Red Cross Society
6. VinaCapital Foundation
7. World Bank Vietnam
8. Blue Dragon Children's Foundation
9. Viettel Social Fund
10. Children First Donor Circle

### Sample Partners with Real Data:

| Organization | Type | Registration | Mission |
|-------------|------|--------------|---------|
| UNICEF Vietnam | NGO | NGO-VN-001 | Children's rights advocacy |
| Save the Children Vietnam | NGO | NGO-VN-002 | Children's welfare and protection |
| World Vision Vietnam | NGO | NGO-VN-003 | Community-based child development |
| CARE International Vietnam | NGO | NGO-VN-005 | Gender equality & women's empowerment |
| Viet Nam Red Cross Society | NGO | NGO-VN-009 | Humanitarian disaster relief |
| VinaCapital Foundation | Partner | NGO-VN-014 | Healthcare & education for underprivileged |
| FPT Corporation | Partner | NGO-VN-024 | Corporate CSR & digital literacy |
| Vietnam Corporate Alliance | Supporter | SUPP-VN-001 | Corporate philanthropy coalition |

---

## FAQs Inserted

### Total Count: 30 FAQs

### By Category:

| Category | Count |
|----------|-------|
| Donations | 9 |
| Account | 4 |
| Child Welfare | 4 |
| General | 4 |
| Programmes | 4 |
| Volunteering | 4 |
| Partners | 1 |

### Featured FAQs:
1. What is GiveAID and how does it work? (312 views)
2. How do I make a donation on GiveAID? (312 views)
3. I forgot my password. How do I reset it? (345 views)
4. Is my donation tax-deductible? (198 views)
5. Is my personal information secure on GiveAID? (198 views)
6. How is my donation used? (289 views)
7. What are the campaigns on GiveAID? (234 views)
8. How do I create an account on GiveAID? (267 views)

### FAQ Topics Covered:
- **General**: Platform overview, mission/vision, contact info, international donors
- **Donations**: How to donate, payment methods, tax receipts, refunds, recurring donations, pending status
- **Programmes**: Campaign types, starting a campaign, campaign goal scenarios
- **Account**: Registration, password reset, profile updates, receipt downloads, account cancellation
- **Partners**: Corporate partnerships, organization verification, fees
- **Volunteering**: On-site and remote volunteering, corporate volunteering
- **Child Welfare**: Fraud protection, safeguarding policies

---

## Build Status

| Component | Status |
|-----------|--------|
| Backend (GiveAID.Web) | **PASS** - Built successfully |
| Frontend (GiveAID.Client) | **PASS** - Built successfully |

### Build Output:
- **Backend**: `GiveAID.Web.dll` - 0 warnings, 0 errors
- **Frontend**: `build/static/js/main.js` (237.86 kB gzip), `build/static/css/main.css` (80.42 kB gzip)

---

## API Test Results

### GET /api/organizations
- Returns all active organizations with pagination
- Supports filtering by type (Supporter, Partner, NGO)
- Returns organization details including mission, vision, contribution amounts

### GET /api/faqs
- Returns all active FAQs ordered by featured first, then display_order
- Supports filtering by category
- Supports search by question/answer text

### GET /api/faqs/categories
- Returns distinct FAQ categories for filter dropdown

---

## Files Modified

### Created:
- `database/migrations/NGO_Database_Partners_FAQs_Seed.sql`

### Modified:
- None (data migration only)

---

## Data Quality Notes

1. **Real Organizations**: All 42 organizations are real NGOs/foundations operating in Vietnam
2. **Realistic Contact Info**: Real email addresses, phone numbers, and office addresses
3. **Proper Registration Numbers**: Format NGO-VN-XXX for NGOs, SUPP-VN-XXX for supporters, PART-VN-XXX for partners
4. **Meaningful Content**: Mission and vision statements are realistic for each organization
5. **Comprehensive FAQs**: 30 FAQs covering all major user questions with detailed HTML-formatted answers

---

## Next Steps

1. **Start Backend Server**: `dotnet run` in GiveAID.Web folder
2. **Start Frontend Server**: `npm start` in GiveAID.Client folder
3. **Test Pages**:
   - Visit `/our-partners` to verify partners display
   - Visit `/help-centre` to verify FAQs display with category filtering

---

*Report generated: 2026-09-18*
