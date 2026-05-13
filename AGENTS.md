# Agentic Coding Guidelines for Maliev.LeaveService

This document defines the standards, commands, and conventions for AI agents working on this repository. Follow these instructions strictly.

## 1. Environment & Build

- **Platform**: .NET 10.0 (C#)
- **Frameworks**: ASP.NET Core, Entity Framework Core, MediatR, MassTransit, Aspire.
- **Database**: PostgreSQL (Npgsql).

### Build, Test & Lint Commands

All commands run from within this service directory (`B:\maliev\Maliev.LeaveService`).

```powershell
# Build (treats warnings as errors — all must be fixed)
dotnet build Maliev.LeaveService.slnx

# Run all tests
dotnet test Maliev.LeaveService.slnx --verbosity normal

# Run a single test method
dotnet test --filter "FullyQualifiedName~Namespace.ClassName.MethodName"

# Run all tests in a class
dotnet test --filter "FullyQualifiedName~Namespace.ClassName"

# Run with code coverage
dotnet test Maliev.LeaveService.slnx --collect:"XPlat Code Coverage"

# Format check
dotnet format Maliev.LeaveService.slnx

# Clean
dotnet clean Maliev.LeaveService.slnx

# EF Core migrations (Infrastructure project only)
dotnet ef migrations add <Name> --project Maliev.LeaveService.Infrastructure --startup-project Maliev.LeaveService.Infrastructure
```

## 2. Architecture & Structure

This project follows **Clean Architecture** with **CQRS** pattern.

```
Maliev.LeaveService/
├── Maliev.LeaveService.Api/           # Controllers, Consumers, Middleware
├── Maliev.LeaveService.Application/   # Use cases, DTOs, Interfaces, Handlers
├── Maliev.LeaveService.Domain/        # Entities, value objects, domain logic
├── Maliev.LeaveService.Infrastructure/ # EF Core DbContext, repositories, HTTP clients
├── Maliev.LeaveService.Tests/         # Unit + Integration tests (xUnit)
├── Directory.Build.props              # Central package versioning
└── Maliev.LeaveService.slnx          # Solution file (.slnx preferred over .sln)
```

- **`Maliev.LeaveService.Api`**: Entry point, Controllers. Minimal logic, delegates to MediatR.
- **`Maliev.LeaveService.Application`**: Business logic. Contains Commands, Queries, Handlers, DTOs, and Interfaces.
  - **Pattern**: Use `MediatR` for all requests.
  - **Results**: Use `CommandResult` (or similar Result pattern) instead of throwing exceptions for business validation.
- **`Maliev.LeaveService.Domain`**: Core entities, Enums, Domain logic.
  - **Dependencies**: None (Pure C#).
- **`Maliev.LeaveService.Infrastructure`**: Implementation of interfaces (Repositories, DbContext, External Services).
- **`Maliev.LeaveService.Tests`**: Unit and Integration tests (xUnit).

## 3. Code Style & Conventions

### C# Naming & Formatting
- **Namespaces**: File-scoped (`namespace Maliev.LeaveService.Api;`)
- **Classes/Methods/Properties**: `PascalCase`
- **Private fields**: `_camelCase` (underscore prefix)
- **Parameters/locals**: `camelCase`
- **Async methods**: Suffix with `Async` (e.g., `GetByIdAsync`), except for MediatR `Handle`
- **Interfaces**: Prefix with `I` (e.g., `ILeaveRepository`)
- **Permissions**: GCP-style `{domain}.{plural-resource}.{action}` as `public const string` in a `Permissions` static class
  - Valid: `leave.leave-requests.create`, `leave.leave-requests.approve`
  - Invalid: `leave.request.create` (singular), `leave.approve` (missing resource)
- **XML docs**: Required on ALL public methods and properties
- **Nullable**: Enabled (`<Nullable>enable</Nullable>`). Use `?` explicitly
- **Imports**: System first, then third-party, then local. Alphabetize within groups. Remove unused `using`
- **Braces**: Allman style (new line) for methods and control structures. Expression-bodied for properties/accessors
- **Indentation**: 4 spaces, LF line endings, UTF-8, trim trailing whitespace
- **Dates**: Use `DateTimeOffset` for all date/time properties. Use `DateTimeOffset.UtcNow`.

### C# Patterns
- **DI**: Constructor injection with `private readonly` fields
- **Controllers**: `[ApiController]`, `[ApiVersion("1")]`, `[Route("leave/v{version:apiVersion}")]`
- **Logging**: `ILogger<T>` with structured placeholders (never interpolate): `_logger.LogInformation("Processing {LeaveRequestId}", leaveRequestId)`
- **Error handling**: Global exception middleware. Return `ProblemDetails` / `ErrorResponse` DTOs. Never expose stack traces
- **JSON**: Check existing conventions in this service for naming policy
- **Manual mapping**: Static extension methods (`ToDto()`, `ToEntity()`). AutoMapper is banned
- **Validation**: `System.ComponentModel.DataAnnotations` on DTOs. FluentValidation is banned
- **Resource scoping**: Employee/manager route and query identifiers must be checked against the authenticated employee claim unless the caller is a service token or leave admin. Do not trust `employeeId`, `requestedBy`, `managerId`, or `approverId` from the request alone.

### Common Patterns (Examples)

**Handler Structure:**
```csharp
public class MyCommandHandler : IRequestHandler<MyCommand, CommandResult>
{
    private readonly IRepository _repo;

    public MyCommandHandler(IRepository repo) => _repo = repo;

    public async Task<CommandResult> Handle(MyCommand request, CancellationToken ct)
    {
        // Validation
        // Logic
        // Return Result
    }
}
```

**Controller Structure:**
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateDto dto)
{
    var command = new CreateCommand { ... };
    var result = await _mediator.Send(command);
    return result.IsSuccess ? Ok(result) : BadRequest(result.ErrorMessage);
}
```

---

## Banned Libraries (Build Will Fail)

| Banned | Use Instead |
|--------|-------------|
| AutoMapper | Manual mapping extensions |
| FluentValidation | DataAnnotations or manual validation |
| FluentAssertions | Standard xUnit `Assert.*` |
| Swashbuckle/Swagger | Scalar (at `/leave/scalar`) |
| InMemoryDatabase (EF Core) | Testcontainers with real PostgreSQL |

---

## 4. Testing Guidelines

- **Framework**: xUnit with standard `Assert` (`Assert.Equal`, `Assert.NotNull`, etc.)
- **Location**: `Maliev.LeaveService.Tests`.
- **Naming**: `MethodName_StateUnderTest_ExpectedBehavior` or `HTTP_METHOD_Path_Scenario_ExpectedStatus`
- **Coverage**: Minimum 80% per service
- **Mocks**: Mock all external dependencies (Repositories, Publishers) in Unit tests.
- **Integration tests**: `BaseIntegrationTestFactory<TProgram, TDbContext>` with Testcontainers (PostgreSQL, Redis, RabbitMQ). Never InMemoryDatabase
- **System tests** (Tier 3): `AspireTestFixture` with `[Collection("AspireDomainTests")]` — tested in `Maliev.Aspire.Tests/`
- **Eventual consistency**: Use `TestHelpers.WaitForAsync`. Never `Task.Delay`
- **MassTransit consumers**: Must have consumer tests using `AddMassTransitTestHarness()`
- Use `[Fact]` for single cases, `[Theory]` for parameterized tests

### Testing Strategy (4-Tier Pyramid Context)

This service's tests cover **Tier 1 (Unit)** and **Tier 2 (Service Integration)** of the Maliev testing pyramid:

| Tier | What to Test | Infrastructure |
|------|-------------|---------------|
| **Unit** | Business logic, domain models, service methods with mocked dependencies | None (mocks only) |
| **Service Integration** | API endpoints, database persistence, permission enforcement, input validation | `BaseIntegrationTestFactory` + Testcontainers (Postgres/Redis/RabbitMQ) |

**Tier 3 (System Integration)** — cross-service workflows and event chains — is tested in `Maliev.Aspire.Tests/`.

> Full ecosystem test strategy: `Maliev.Aspire.Tests/TEST_PLAN.md`

---

## 5. Workflow for Agents

1. **Explore**: Identify relevant files using `ls -R` or `glob`.
2. **Read**: Read related files (Entities, Handlers, Tests) to understand context.
3. **Plan**: Propose a plan including files to modify and tests to add/update.
4. **Edit**: Apply changes using `edit` or `write`. **Respect existing formatting**.
5. **Verify**:
   - **Always** run the specific test you added/modified: `dotnet test --filter "FullyQualifiedName~..."`
   - If creating new files, ensure they are added to the `.csproj` (usually automatic in .NET, but verify file location).
   - Fix any compilation errors immediately.

---

## 6. Mandatory Rules

- **`TreatWarningsAsErrors = true`**: Zero warnings allowed. No suppression
- **`[RequirePermission("leave.leave-requests.action")]`**: On all endpoints, not plain `[Authorize]`
- **API versioning**: All routes versioned (`v1/`)
- **Service prefix**: Routes prefixed with `/leave`
- **Scalar docs**: Configured at `/leave/scalar`
- **Secrets**: Never hardcoded. Use GCP Secret Manager or environment variables
- **Async/await**: All the way down. Pass `CancellationToken`
- **EF Core Design package**: Only in Infrastructure project, never in Api
- **PostgreSQL xmin**: Shadow property only — `entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion()`. Never add entity property
- **Temporary files**: Generate in `/temp` folder, clean up afterwards

### PostgreSQL xmin Concurrency — Mandatory Pattern
Use shadow property ONLY. Never add a Xmin/xmin property to domain entities.
```csharp
entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
```
- Never use `UseXminAsConcurrencyToken()` (removed in Npgsql EF v7)
- Never use entity property `public uint Xmin { get; set; }` or `public uint xmin { get; set; }`
- Never use `.Ignore(e => e.Xmin)` — remove the entity property instead

---

## 7. Git Rules

- This is an independent git repo. Work from `B:\maliev\Maliev.LeaveService` for git commands
- **Commit early and often** after every meaningful unit of work. Do not accumulate changes
- **Never use `git checkout` to restore files** — commit first, then `git revert` or `git reset --soft`
- Feature branches merged to `develop` via PR. Do not push without being asked
