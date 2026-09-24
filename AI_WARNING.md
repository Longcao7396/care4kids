# AI_WARNING.md — Critical Mistakes to Avoid When Editing This Codebase

> **Read this BEFORE making any non-trivial change.** These are bugs that were painful to debug once. Don't re-introduce them.

---

## 🚨 1. Login fails with "Invalid username or password" for newly registered users

**Cause**: User has `IsVerified=false` in DB. Login handler rejects unverified users.

**Fix** (already applied): `Email:RequireVerification=false` in `appsettings.Development.json` auto-verifies new registrations in dev. Production sets it to `true`.

**If a user reports this bug again**:
1. Check the user's `is_verified` column in the `Users` table.
2. If 0, run: `UPDATE Users SET is_verified=1 WHERE is_verified=0`
3. Verify `Email:RequireVerification` is being read correctly:
   ```csharp
   services.Configure<EmailOptions>(o => o.RequireVerification = bool.TryParse(
       configuration["Email:RequireVerification"], out var v) ? v : true);
   ```

**DO NOT** "fix" this by removing the `IsVerified` check from `LoginCommandHandler`. Email verification is a security feature.

---

## 🚨 2. `UnauthorizedAccessException` was being swallowed by middleware

**Cause**: `ExceptionHandlingMiddleware` returned generic "Unauthorized access" instead of preserving the handler's exception message.

**Fix** (already applied): `AuthController.Login` now catches `UnauthorizedAccessException` explicitly and returns `{ success:false, message: ex.Message, code:"INVALID_CREDENTIALS" }`.

**DO NOT** "fix" this by removing the middleware or making it always pass through exceptions. The middleware handles cross-cutting concerns (logging, sanitization). Fix it at the controller layer where business errors originate.

---

## 🚨 3. `authService.login()` was swallowing server errors

**Cause**: Old code had `try { ... } catch { return null }`, causing `AuthContext` to never see server-side error messages.

**Fix** (already applied): Removed the catch. `authService.login()` re-throws on failure so `AuthContext.login()` can read `err.response.data.message` and `err.response.data.code`.

**DO NOT** "fix" this by adding a catch that returns null. The frontend MUST propagate server errors.

---

## 🚨 4. .NET 10 Application project cannot reference `Microsoft.Extensions.Configuration.Binder`

**Cause**: We added `IConfiguration` injection to `RegisterCommandHandler`. The Application layer doesn't have a transitive reference to the binder package.

**Fix** (already applied): Added these packages to `GiveAID.V2.Application.csproj`:
```xml
<PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="10.0.12" />
<PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="10.0.12" />
```

**But we also switched to `IOptions<EmailOptions>` pattern** (cleaner). The Binder package is still needed for the `.Value` resolution.

**DO NOT** move `EmailOptions` into `WebApi` (creates wrong dependency direction). Keep it in `Application/Common/Interfaces/`.

---

## 🚨 5. LocalDB connection uses `Microsoft.Data.SqlClient` (managed), not native

The backend has a NuGet.config / runtimeconfig that forces managed networking. If the backend crashes with `STATUS_DLL_NOT_FOUND` on a tester machine, this is the cause.

**Fix**: Already configured. If regressed, check:
- `src/WebApi/runtimeconfig.json` should NOT have `System.GC.Server` or native AOT settings
- No `Microsoft.Data.SqlClient.SqlClientFactory` registration conflict

**DO NOT** add `Microsoft.Data.SqlClient` directly. Use `Microsoft.EntityFrameworkCore.SqlServer` which transitively pulls the right version.

---

## 🚨 6. Backend `SeedData.cs` had hardcoded dev passwords

**Cause**: Original code had `?? "DevAdmin@123"` and `?? "Demo@123"` as fallback for the seed users. These got committed and pushed.

**Fix** (already applied): Removed hardcoded fallbacks. Now throws if `ADMIN_PASSWORD` env var is not set. `START.bat` sets it for dev.

**When migrating a new environment**:
```bash
export ADMIN_PASSWORD='YourSecurePassword'  # min 8 chars
export Jwt__Secret='32+chars-of-random-string'
```

**DO NOT** add back the hardcoded fallback "for convenience". It's a security regression.

---

## 🚨 7. FluentValidation validators exist but are NOT enforced

The validators (`LoginCommandValidator`, `RegisterCommandValidator`, etc.) are registered in DI but no pipeline behavior runs them.

**If you add a new validator**: Either:
- Add `services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))` to `AddApplicationServices()` (and create `ValidationBehavior<,>`), OR
- Manually invoke the validator inside the handler:
  ```csharp
  var validator = new LoginCommandValidator();
  var result = await validator.ValidateAsync(request, cancellationToken);
  if (!result.IsValid) throw new ValidationException(result.Errors);
  ```

**DO NOT** assume validator is automatically called just because it exists in the assembly.

---

## 🚨 8. `package.json` "start" script does NOT just start React

```json
"start": "concurrently -k -n BACKEND,FRONTEND -c blue,green \"npm run start:backend\" \"wait-on http://localhost:5231/healthz && npm run start:frontend\""
```

Running `npm start` from `GiveAID.Client/` starts BOTH backend and frontend with color-coded output. Press Ctrl+C once to stop both.

**DO NOT** "simplify" this to just `react-scripts start`. You'll break the dev workflow.

---

## 🚨 9. Database schema uses snake_case columns

EF Core is configured to map `IsVerified` (PascalCase) → `is_verified` (snake_case) column. SQL queries must use snake_case.

**If you write raw SQL**:
```sql
SELECT user_id, username, is_verified FROM [dbo].[Users]  -- ✅
SELECT UserId, IsVerified FROM dbo.Users                    -- ❌ throws "Invalid column name"
```

**DO NOT** change EF Core conventions back to PascalCase columns. The snake_case convention is consistent across all migrations and the DB seed scripts.

---

## 🚨 10. JWT secret in production MUST come from env var, never config

`appsettings.json` has empty `"Secret": ""` for Jwt. `Program.cs` validates and throws if `Jwt__Secret` env var is missing in non-Development.

**Test in production**:
```bash
dotnet run --environment Production
# Should throw: "Jwt:Secret is required. Set the Jwt__Secret environment variable."
```

**DO NOT** put any value in `appsettings.json` Secret. Even in Development, set via env var (`START.bat` does this).

---

## Quick Reference: Files that were changed during v2.0.0 stabilization

| File | Why | Risk if reverted |
|---|---|---|
| `AuthController.cs` | Added `catch (UnauthorizedAccessException)` | Login returns generic "Unauthorized access" |
| `AuthContext.js` | Reads `err.response.data.message` + `code` | Login shows wrong error message |
| `authService.js` | Removed try/catch swallowing | Login errors invisible to UI |
| `RegisterCommandHandler.cs` | Reads `IOptions<EmailOptions>` | New users stuck with `IsVerified=false` |
| `IEmailOptions.cs` (new) | Defines `EmailOptions` | Compilation fails |
| `SeedData.cs` | Removed hardcoded passwords | Dev seeds never run; security regression |
| `START.bat` | Sets env vars before launching | Seed fails: "ADMIN_PASSWORD is required" |
| `appsettings.Development.json` | `Email:RequireVerification=false` | Dev users can't log in |

---

## If you're unsure, ASK

Don't guess. Don't "fix" by reverting. Don't add a workaround that masks the root cause. Read `AI_GUIDE.md` for the broader picture, then ask the developer.
