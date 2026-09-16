# Coding Conventions

> Audience: AI coding assistants and human contributors. Follow these to keep the codebase
> consistent and easy to upgrade.

---

## Backend (C# / .NET Framework 4.7.2)

### 1. Naming

| Element | Convention | Example |
|---|---|---|
| Class | `PascalCase` | `CampaignsController` |
| Method | `PascalCase` | `GetDashboardStats` |
| Public property | `PascalCase` | `CampaignName` |
| Private field | `_camelCase` | `_validationContext` |
| Local variable | `camelCase` | `campaignList` |
| Constant | `PascalCase` | `AdminHash` |
| Enum | `PascalCase` (member also) | `CampaignStatus.Active` |
| Interface | `IPascalCase` | `IJwtTokenProvider` |
| Route prefix | `kebab-case` | `api/campaigns` |
| Attribute | `PascalCase` | `[JwtAuthorize]` |

### 2. Controllers

- One controller per top-level resource. Sub-resources get nested routes.
- All controller actions returning entities **must** use a DTO from `Models/CampaignDto.cs`
  or `Models/ViewModels.cs`. Never return raw EF entities to the client.
- `[RoutePrefix]` is mandatory. Don't rely on action-name conventions.
- Use `[HttpGet]`, `[HttpPost]`, etc. for explicit verb mapping.
- Always wrap multi-step operations in `using (var ctx = new GiveAIDContext())` if not using DI
  (this project doesn't use Unity/Autofac — context is created per request).

**Required attributes per endpoint type:**

| Endpoint type | Attribute |
|---|---|
| Public read | (none) |
| Public write (login, register) | (none) but **always validate input manually** |
| Authenticated user | `[JwtAuthorize]` |
| Admin only | `[JwtAuthorize(Roles = "SuperAdmin,Admin")]` |
| SuperAdmin only | `[JwtAuthorize(Roles = "SuperAdmin")]` |
| Owner-or-admin (e.g. own donation) | `[JwtAuthorize]` + manual `UserId` check inside the action |

### 3. Error responses

Every action returns `IHttpActionResult`. Use `Ok(data)`, `BadRequest(msg)`, `Unauthorized()`,
`Content(HttpStatusCode.Xxx, body)`. The body **must** follow the shape:

```csharp
new { success = true/false, message = "...", data = ... }
```

Don't return plain strings or naked objects — the frontend services assume the envelope.

### 4. LINQ & performance

- **Avoid N+1.** If you need related data, use `.Include(...)` or pre-fetch into a `Dictionary`
  (see `CampaignsController` `causeLookup` pattern).
- Use `.AsNoTracking()` for read-only queries (this codebase doesn't always do this — consider
  adding for hot paths).
- Project to DTOs inside the LINQ query (`Select(...)`) to avoid materialising full entities.

### 5. Money

- Always `decimal`, never `double` or `float`.
- Use `HasPrecision(18, 2)` in EF fluent config (see `GiveAIDContext.OnModelCreating`).
- Currency: VND. Symbol: `₫` (Unicode U+20AB, NOT `VND` or `d`).

### 6. Security

- **Never** log raw JWT, password, or card data. Use `Debug.WriteLine("[Component] message: " + ...)` carefully.
- **Never** store raw card numbers. Use `PaymentToken` + `CardLast4`.
- Always check `user.IsActive` in `JwtHelper.ValidateToken` (it does — don't bypass).
- Prefer `[Authorize]`-equivalent checks via `[JwtAuthorize]`. Don't add raw OWIN bearer.

### 7. Entity models (`EntityModels.cs`)

- One file, one namespace (`GiveAID.Web.Models`).
- Entity name = table name (PascalCase).
- Snake-case is automatic — don't add `.HasColumnName(...)` unless breaking convention.
- Navigation properties: `Cause` (singular) and `Causes` (collection).
- Soft-delete columns named `is_active` (bool, default `true`).
- Audit columns: `created_at` (DateTime), `updated_at` (DateTime?).
- Always use `[Table("...")]` to make SQL table explicit.

### 8. Comments

- Use XML doc comments (`///`) on all public classes and methods.
- Don't add inline comments to explain *what* the code does — the code should be self-evident.
- Add inline comments to explain *why* a non-obvious decision was made.

---

## Frontend (React 18 / JavaScript)

### 1. Naming

| Element | Convention | Example |
|---|---|---|
| Component file | `PascalCase.js` | `CampaignCard.js` |
| Component | `PascalCase` | `function CampaignCard()` |
| Hook | `camelCase` (starts with `use`) | `useAuth` |
| Constant | `UPPER_SNAKE` or `PascalCase` for objects | `API_ENDPOINTS`, `STATUS.Active` |
| Service method | `camelCase` | `campaignsService.getAll` |
| CSS class | `kebab-case` (BEM-like) | `c4k-card__title` |
| CSS file | `PascalCase.css` matching component | `CampaignCard.css` |
| Folder | `camelCase` or `kebab-case` | `pages/admin/` |

### 2. Components

- **One component per file.** Default-export the main component.
- Pages go in `pages/` or `pages/admin/`. Reusable UI goes in `components/`.
- Pages compose components; components don't import from `pages/`.
- Admin pages **must** wrap their content in `<AdminPageFrame>` (provides header + banners).

### 3. State

- Local state via `useState`.
- Shared state via `useContext` (`AuthContext` is the only context today; add new ones in `contexts/`).
- Server state via local `useEffect` + `useState` (no Redux/React Query yet — keep it simple).
- Always clean up `useEffect` (cancel flag, `AbortController`, or unsubscribe).

### 4. API access

- **Never call axios directly in components.** Go through `services/index.js`.
- If you need a new endpoint, add it to `services/index.js` + add the URL to `config.js`'s
  `API_ENDPOINTS` map.
- Always wrap async work in try/catch and surface errors via local `error` state.
- The `api.js` interceptor handles 401 → redirect to `/login`. Don't reinvent it.

### 5. Styling

- **Bootstrap 5 + react-bootstrap first.** Use their components, classes, and utilities.
- Custom CSS lives next to the component (`Foo.js` + `Foo.css`).
- Custom design tokens are CSS custom properties on `:root` (see `styles/global.css`):
  - `--c4k-teal`, `--c4k-coral`, `--c4k-warm-yellow`, `--c4k-ink`, `--c4k-cream`
- **Never** inline `style={{ color: '#…' }}` — use a CSS class with a token.
- **Grid layout**: use CSS Grid for full-row layouts (`display: grid; grid-template-columns:
  repeat(N, 1fr)`) and Bootstrap `Row`/`Col` for content that needs Bootstrap's gutters.

### 6. Accessibility

- Every interactive element must have an accessible name (label, aria-label, or visible text).
- Form inputs: wrap in `<Form.Group controlId="…">` so labels are wired up.
- Modals: react-bootstrap's `<Modal>` handles focus trap — don't roll your own.
- Colour contrast: never use the coral/red colour for body text; it's a brand accent only.

### 7. Linting

ESLint config (`.eslintrc` equivalent in `package.json`) enforces:

- No unused imports / variables
- No BOM at start of file
- React hooks rules

**Always run `npx eslint src/` before committing.** The current baseline is **0 errors, 0 warnings**.

### 8. Imports

Order (enforced by ESLint `import/order`):

1. React core (`react`, `react-dom`, `react-router-dom`)
2. Third-party (`react-bootstrap`, `axios`, …)
3. Project (`../components/...`, `../services`, `../contexts/...`)
4. Relative (`./Foo.css`)

### 9. File headers

No copyright headers. Keep the first line of every file as the import block.

---

## Database (SQL)

### 1. Migration scripts

- One script per logical change. Name with `NGO_Database_<Topic>_<Action>.sql`.
- New scripts live in project root, NOT inside `GiveAID.Web/`.
- **Always idempotent** — use `IF NOT EXISTS`, `IF OBJECT_ID('…') IS NOT NULL`, etc.
- Wrap destructive operations in `BEGIN TRANSACTION` + `BEGIN TRY / END TRY / BEGIN CATCH /
  ROLLBACK` for atomicity.
- Add a `PRINT` at the end so the operator sees confirmation.

### 2. Data seeding

- Seed scripts use `MERGE` keyed on a stable business identifier (`campaign_code`, `cause_code`)
  — never on auto-increment IDs.
- Resolve IDs dynamically: `DECLARE @EDU_ID INT = (SELECT cause_id FROM Causes WHERE cause_code = 'EDU')`.
- Guard donations with `IF NOT EXISTS (SELECT 1 FROM Donations WHERE transaction_id = 'TXN-…')`.

### 3. Naming

- Tables and columns: `snake_case`
- Constraints: `FK_<Table>_<ReferencedTable>_<Column>` (FK), `CHK_<Table>_<Rule>` (CHECK),
  `IX_<Table>_<Column>` (index), `DF_<Table>_<Column>` (default)

---

## Git / commits

- Branch name: `feat/<short-desc>`, `fix/<short-desc>`, `chore/<short-desc>`
- Commit messages: imperative present tense ("Add / Fix / Refactor / Update")
- One logical change per commit. Don't bundle unrelated fixes.

---

## When unsure

- Read `ARCHITECTURE.md` first.
- Search for an existing pattern in the codebase before inventing a new one.
- Prefer simple, boring solutions over clever ones.
