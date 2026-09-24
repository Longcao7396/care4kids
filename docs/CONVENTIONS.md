# Coding Conventions — GiveAID v2.0

These conventions keep the codebase consistent and reviewable. They apply to all C# code
in `src/` and `tests/`.

## 1. C# Style

### 1.1. Naming

| Element                | Convention                  | Example                            |
|------------------------|-----------------------------|------------------------------------|
| Namespace              | PascalCase, dot-separated    | `GiveAID.Application.Features.Auth` |
| Class                  | PascalCase                   | `LoginCommandHandler`              |
| Interface               | `I` + PascalCase             | `IJwtTokenService`                 |
| Method                 | PascalCase                   | `GetCampaignsAsync`                |
| Property               | PascalCase                   | `EmailAddress`                     |
| Private field          | `_camelCase`                 | `_httpClient`                      |
| Local variable         | camelCase                    | `campaignId`                       |
| Constant               | PascalCase                   | `DefaultPageSize`                  |
| Enum value             | PascalCase                   | `CampaignStatus.Active`            |
| Async method           | suffix `Async`               | `HandleAsync`                      |

### 1.2. File Layout

```
// Copyright header (optional)
using System.Text;          // System.*
using Microsoft.Extensions.*; // Third-party
using GiveAID.Domain.*;       // Project
namespace GiveAID.Application.Features.Campaigns;

public class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateCampaignCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<int>> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        // ...
    }
}
```

### 1.3. Braces

Allman style (brace on its own line):

```csharp
public void Method()
{
    if (condition)
    {
        DoSomething();
    }
}
```

### 1.4. `var` vs Explicit Type

Use `var` when the type is obvious from the right-hand side:

```csharp
var user = new User();              // OK
var count = _context.Users.Count(); // OK — clear
int count = _context.Users.Count();  // OK if you prefer explicit
```

Use explicit type when the type isn't obvious:

```csharp
IEnumerable<Campaign> result = await _mediator.Send(query); // explicit
```

## 2. Nullable Reference Types

`Nullable` is enabled project-wide. All reference types are non-nullable by default.

```csharp
public string Name { get; set; } = string.Empty;   // never null
public string? Description { get; set; }            // explicitly nullable
```

Rules:
- Initialise non-nullable strings to `string.Empty` or a sensible default
- Initialise non-nullable collections to `new List<T>()` or `Array.Empty<T>()`
- Avoid `null!` unless absolutely necessary (and document why)

## 3. Project Structure

```
<Project>/
├── <Project>.csproj
├── Common/
├── Features/<FeatureName>/
│   ├── Commands/<Action>/
│   └── Queries/<Action>/
├── Interfaces/
└── Services/
```

Each handler lives in its own file: `LoginCommand.cs`, `LoginCommandHandler.cs`,
`LoginCommandValidator.cs`. One public type per file.

## 4. CQRS Patterns

### 4.1. Command

```csharp
public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;
```

Use `record` for immutable value semantics.

### 4.2. Handler

```csharp
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        // ...
        return Result.Success(response);
    }
}
```

### 4.3. Validator

```csharp
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
```

### 4.4. Query

```csharp
public record GetCampaignsQuery(int Page = 1, int PageSize = 10) : IRequest<PagedResult<CampaignDto>>;
```

Queries don't return `Result<T>` — they either return data or throw `NotFoundException`.

## 5. Error Handling

- **Don't** catch `Exception` unless you re-throw or log + translate
- **Throw** domain-specific exceptions: `ValidationException`, `NotFoundException`,
  `UnauthorizedAccessException`
- **Let the middleware** translate exceptions into HTTP responses

```csharp
public async Task<User> Handle(GetUserByIdQuery query, CancellationToken ct)
{
    var user = await _context.Users.FindAsync(query.Id, ct);
    return user ?? throw new NotFoundException($"User {query.Id} not found");
}
```

## 6. Async/Await

- Use `async`/`await` everywhere an I/O call exists
- Append `Async` to method names that return `Task`/`Task<T>`
- Don't use `.Result` or `.Wait()` — they deadlock under sync contexts

```csharp
public async Task<Campaign> GetByIdAsync(int id, CancellationToken ct)
{
    return await _context.Campaigns.FindAsync(new object[] { id }, ct);
}
```

## 7. Entity Framework Core

### 7.1. No Lazy Loading

Use eager loading with `Include`:

```csharp
var campaign = await _context.Campaigns
    .Include(c => c.Cause)
    .Include(c => c.Donations)
    .FirstOrDefaultAsync(c => c.Id == id, ct);
```

### 7.2. Async Querying

Always `await` queries; never `.ToList()` synchronously.

### 7.3. Configuration

Entity configurations live in `src/Infrastructure/Persistence/Configurations/`, **not**
data annotations on entities.

```csharp
public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.HasOne(c => c.Cause).WithMany().HasForeignKey(c => c.CauseId);
    }
}
```

## 8. Tests

- One assertion concept per `[Fact]`
- AAA structure with blank lines between
- FluentAssertions: `result.Should().Be(...)`
- Mock dependencies with Moq
- For data setup, use builders or AutoFixture

```csharp
[Fact]
public async Task Login_WithValidCredentials_ReturnsToken()
{
    // Arrange
    var user = new User { Email = "test@example.com", IsActive = true };
    _ctx.Setup(c => c.Users).Returns(BuildMockDbSet(new[] { user }).Object);

    // Act
    var result = await _handler.Handle(new LoginCommand("test@example.com", "pwd"), default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Token.Should().NotBeNullOrEmpty();
}
```

## 9. Git Workflow

### Branch naming

```
feature/<ticket>-<short-description>
bugfix/<ticket>-<short-description>
chore/<short-description>
release/<version>
```

### Commit messages — Conventional Commits

```
feat(auth): add password reset endpoint
fix(donations): handle Stripe timeout gracefully
docs(readme): update quick start
test(campaigns): add progress calculation tests
chore(deps): bump MediatR to 12.2.0
```

Format: `<type>(<scope>): <description>`

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`.

### Pull request checklist

- [ ] Build passes locally (`dotnet build`)
- [ ] All tests pass (`dotnet test`)
- [ ] No new warnings
- [ ] Migration script added (if DB schema changed)
- [ ] Documentation updated (if API changed)
- [ ] Self-reviewed before requesting review
- [ ] PR description explains **why**, not just **what**

## 10. Logging

Use `ILogger<T>` with structured logging:

```csharp
_logger.LogInformation("Campaign {CampaignId} created by user {UserId}", id, userId);
```

Log levels:
- `Trace` / `Debug` — verbose, off in production
- `Information` — normal flow (created, updated, sent)
- `Warning` — recoverable issues (retries, deprecated usage)
- `Error` — handled exceptions, recoverable failures
- `Critical` — application-level failures, requires alerting

**Don't log:**
- Passwords (even hashed)
- JWT tokens
- Credit card numbers (use last 4 only)
- PII without consent

## 11. Security

- All inputs are untrusted — validate with FluentValidation
- All outputs to HTML are sanitised (Ganss.XSS)
- All SQL goes through EF Core (no string concatenation)
- JWT secret must be ≥ 64 bytes
- Use `[Authorize]` + policies (`Admin`) on protected endpoints
- CORS allow-list is explicit, never `*` in production
- Rate limit applied at `/api/v1/*` by default

## 12. Performance

- Use `AsNoTracking()` for read-only queries
- Paginate lists — never return unbounded collections
- Index foreign keys and frequently filtered columns (configured in migrations)
- Use `ResponseCache` for read-heavy endpoints
- Use `MemoryCacheService` for hot data (statistics, lookups)

## 13. Don't Do

- ❌ Reference `Infrastructure` from `Application`
- ❌ Reference `WebApi` from `Domain` or `Application`
- ❌ Catch `Exception` broadly without re-throwing
- ❌ Use `async void` (except event handlers)
- ❌ Use `.Result` / `.Wait()`
- ❌ Store secrets in source control
- ❌ Commit `appsettings.Development.json` with real credentials
- ❌ Disable nullable warnings with `!` (without justification)

## 14. Code review etiquette

- Review **the change, not the author**
- Be specific: "Add a null check on `user.Email`" beats "this might break"
- Approve if you wouldn't change anything significant
- Ask questions rather than demand changes when uncertain
