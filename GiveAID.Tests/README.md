# GiveAID.Tests

Unit and integration tests for the Give-AID NGO platform backend (`GiveAID.Web`).

## 📋 Prerequisites

- **.NET Framework 4.7.2** SDK (the project targets `net472`)
- **Visual Studio 2019/2022** with the "ASP.NET and web development" workload, **or**
- **msbuild** on the command line
- **SQL Server LocalDB** (optional — required only for integration tests that use the real EF6 pipeline; unit tests run with no DB)

## 🚀 Running Tests

### Via Visual Studio Test Explorer
1. Open `GiveAID.sln` in Visual Studio
2. Set `GiveAID.Tests` as the startup project
3. Build the solution (`Ctrl+Shift+B`)
4. Open **Test Explorer** (`Test → Windows → Test Explorer`)
5. Click **Run All**

### Via dotnet / msbuild (command line)

```bash
# Restore NuGet packages
nuget restore GiveAID.sln

# Build
msbuild GiveAID.sln /p:Configuration=Debug /t:Build

# Run tests (requires vstest.console.exe from VS or dotnet test)
packages\xunit.runner.console.2.4.2\tools\net452\vstest.console.exe GiveAID.Tests\bin\Debug\GiveAID.Tests.dll
```

### Via `dotnet test` (requires .NET Core SDK + `Microsoft.NET.Sdk` style project)
> Note: GiveAID.Tests is a `net472` project and does not use the .NET Core SDK.
> Install the full Visual Studio build tools to use `vstest.console.exe`.

## 📁 Project Structure

```
GiveAID.Tests/
├── GiveAID.Tests.csproj          # Project file (targets net472, xUnit + Moq)
├── app.config                    # Test connection strings + JWT / SMTP settings
├── packages.config               # NuGet package versions
├── Properties/
│   └── AssemblyInfo.cs
├── Tests/
│   ├── TestDbContextFactory.cs   # Shared DB setup + seed data
│   ├── TestJwtSettings.cs         # Test JWT config constants
│   ├── Unit/
│   │   ├── PasswordHasherTests.cs   # BCrypt hashing
│   │   ├── JwtHelperTests.cs        # JWT generation, validation, claims
│   │   ├── HtmlSanitizerTests.cs    # XSS defence
│   │   ├── RateLimiterTests.cs      # IP-based rate limiting
│   │   └── EmailServiceTests.cs     # Mock SMTP mode
│   └── Integration/
│       ├── AuthFlowTests.cs          # Register, Login, Logout, Me
│       ├── DonationFlowTests.cs      # Create, Confirm, Webhook, Idempotency
│       └── CampaignFlowTests.cs      # CRUD + Registration
└── README.md
```

## 🧪 Test Categories

### Unit Tests (no DB required)
| Class | Target | Key Behaviours |
|-------|--------|----------------|
| `PasswordHasherTests` | `PasswordHasher` | BCrypt hashing, random salt per call, work factor, edge cases |
| `JwtHelperTests` | `JwtHelper` | Token generation, claim correctness, tampered/expired token rejection |
| `HtmlSanitizerTests` | `HtmlSanitizer` | Script removal, event handler stripping, URI scheme filtering |
| `RateLimiterTests` | `RateLimiter` | Under/over limit, IP separation, X-Forwarded-For |
| `EmailServiceTests` | `EmailService` | Mock mode always succeeds, subject/body formatting |

### Integration Tests (requires LocalDB)
| Class | Target | Key Behaviours |
|-------|--------|----------------|
| `AuthFlowTests` | `AuthController` | Register (new/duplicate), Login (valid/wrong/inactive), Logout |
| `DonationFlowTests` | `DonationsController` | Create, ConfirmPayment, Webhook, Idempotency, IDOR defence |
| `CampaignFlowTests` | `CampaignsController` | CRUD (admin-only), Register, MyRegistrations, Delete guards |

## ⚙️ Configuration

All test settings live in `app.config`:

```xml
<connectionStrings>
  <!-- Integration tests use LocalDB. Create with: -->
  <!-- sqllocaldb create GiveAIDTest -->
  <add name="GiveAIDContext"
       connectionString="Data Source=(localdb)\GiveAIDTest;..."
       providerName="System.Data.SqlClient" />
</connectionStrings>

<appSettings>
  <!-- JWT — must match the values GiveAID.Web uses in Web.config -->
  <add key="JwtSecret"        value="TestSecretKeyThatIsAtLeast32CharactersLong!!" />
  <add key="JwtIssuer"        value="GiveAID.Test" />
  <add key="JwtAudience"       value="GiveAID.Test" />
  <add key="JwtExpiryMinutes" value="60" />

  <!-- Email runs in MOCK mode — no real SMTP needed -->
  <add key="SmtpEnabled"      value="false" />
</appSettings>
```

## 🔑 Test Users

| Email | Password | Role |
|-------|----------|------|
| `admin@test.com` | `Admin@123` | SuperAdmin |
| `user@test.com` | `User@123` | User |

The BCrypt hashes for these accounts are:
- `Admin@123` → `$2a$11$pirnEfNk.ZU71wnXOvS99uJklL0iBPrhxTq0watPsNLDhuVtW6Wny`
- `User@123`  → `$2a$11$D2ZJOxRrjuq25IW.5OeOzuVNv8r4GAj8SH7zxXBWpAUu4ZmKvUIvi`

## 📊 Coverage Goals

| Component | Target | Status |
|-----------|--------|--------|
| `PasswordHasher` | 100% | ✅ |
| `JwtHelper` | 90%+ | ✅ |
| `HtmlSanitizer` | 100% | ✅ |
| `RateLimiter` | 80%+ | ✅ |
| `EmailService` | 70%+ | ✅ |
| `AuthController` | 80%+ | ✅ |
| `DonationsController` | 70%+ | ✅ |
| `CampaignsController` | 70%+ | ✅ |

> Coverage reports require the Visual Studio coverage tooling or a third-party tool
> such as **Coverlet** (requires .NET Core SDK) or **OpenCover**.

## 🔮 Future Work

- **UI/E2E tests** — add Selenium or Playwright tests for critical user flows
- **Effort in-memory provider** — add `Effort.EF6` package to run integration
  tests without LocalDB (faster CI)
- **Parameterized tests for campaign status** transitions
- **Performance benchmarks** — measure BCrypt hashing time across hardware

## 📝 Adding New Tests

1. **Unit test**: add a `.cs` file under `Tests/Unit/` with `[Fact]` or `[Theory]` methods.
2. **Integration test**: add a `.cs` file under `Tests/Integration/` with `[Fact]`.
   Inject the shared `GiveAIDContext` via the constructor and call
   `TestDbContextFactory.ResetDatabase(_context)` to get a clean slate.
3. If your test needs a specific seeded entity (campaign, cause, user), add a
   factory method to `TestDbContextFactory.cs` rather than duplicating setup code.

## 🔒 Security Notes

- Test credentials are **never** the same as production credentials.
- BCrypt hashes in test fixtures are generated with the **same cost factor (11)**
  as production, so timing is realistic.
- Integration tests run against a **LocalDB** instance that is **isolated** from
  any staging or production database.
