# Agentic Coding Guidelines for Maliev.LeaveService

This document defines the standards, commands, and conventions for AI agents working on this repository. Follow these instructions strictly.

## 1. Environment & Build

- **Platform**: .NET 10.0 (C#)
- **Frameworks**: ASP.NET Core, Entity Framework Core, MediatR, MassTransit, Aspire.
- **Database**: PostgreSQL (Npgsql).

### Key Commands

| Action | Command | Notes |
| :--- | :--- | :--- |
| **Build Solution** | `dotnet build` | Builds the entire solution. |
| **Run API** | `dotnet run --project Maliev.LeaveService.Api` | Runs the main API project. |
| **Run All Tests** | `dotnet test` | Runs all unit and integration tests. |
| **Run Single Test** | `dotnet test --filter "FullyQualifiedName=Namespace.ClassName.MethodName"` | **Critical**: Use this to verify specific changes. |
| **Run Tests in File** | `dotnet test --filter "FullyQualifiedName~Namespace.ClassName"` | Runs all tests in a specific class. |
| **Watch Tests** | `dotnet watch test --project Maliev.LeaveService.Tests` | Useful for TDD loops. |
| **Clean** | `dotnet clean` | Cleans build artifacts. |

## 2. Architecture & Structure

This project follows **Clean Architecture** with **CQRS** pattern.

- **`Maliev.LeaveService.Api`**: Entry point, Controllers. Minimal logic, delegates to MediatR.
- **`Maliev.LeaveService.Application`**: Business logic. Contains Commands, Queries, Handlers, DTOs, and Interfaces.
  - **Pattern**: Use `MediatR` for all requests.
  - **Results**: Use `CommandResult` (or similar Result pattern) instead of throwing exceptions for business validation.
- **`Maliev.LeaveService.Domain`**: Core entities, Enums, Domain logic.
  - **Dependencies**: None (Pure C#).
- **`Maliev.LeaveService.Infrastructure`**: Implementation of interfaces (Repositories, DbContext, External Services).
- **`Maliev.LeaveService.Tests`**: Unit and Integration tests (xUnit).

## 3. Code Style & Conventions

### formatting & Syntax
- **Namespaces**: Use **file-scoped namespaces** (e.g., `namespace Maliev.LeaveService.Api;` not block-scoped).
- **Indentation**: 4 spaces.
- **Nullable**: Enable nullable reference types (`string?`, `int?`).
- **Dates**: Use `DateTimeOffset` for all date/time properties. Use `DateTimeOffset.UtcNow`.

### Naming
- **Classes/Methods**: `PascalCase` (e.g., `SubmitLeaveRequest`).
- **Variables/Params**: `camelCase` (e.g., `leaveRequest`).
- **Private Fields**: `_camelCase` (e.g., `_repository`).
- **Async Methods**: Suffix with `Async` (e.g., `GetByIdAsync`), except for MediatR `Handle`.
- **Interfaces**: Prefix with `I` (e.g., `ILeaveRepository`).

### Implementation Details
- **Dependency Injection**: Constructor injection only. Assign to `readonly` private fields.
- **Logging**: Inject `ILogger<T>`. Log meaningful events.
  - usage: `_logger.LogInformation("Processing {Id}...", id);` (Structured logging).
- **Validation**: Perform validation in the Handler or via FluentValidation (if present).
- **Comments**: XML documentation (`///`) for public APIs and Domain entities.

## 4. Testing Guidelines

- **Framework**: xUnit + Moq.
- **Location**: `Maliev.LeaveService.Tests`.
- **Naming**: `MethodName_State_ExpectedBehavior` (e.g., `Handle_InsufficientBalance_ShouldReturnFailure`).
- **Pattern**: Arrange, Act, Assert.
- **Mocks**: Mock all external dependencies (Repositories, Publishers) in Unit tests.
- **Integration Tests**: Use `Testcontainers` (if available) or in-memory DB for integration tests.

## 5. Workflow for Agents

1.  **Explore**: identifying relevant files using `ls -R` or `glob`.
2.  **Read**: Read related files (Entities, Handlers, Tests) to understand context.
3.  **Plan**: Propose a plan including files to modify and tests to add/update.
4.  **Edit**: Apply changes using `edit` or `write`. **Respect existing formatting**.
5.  **Verify**:
    *   **Always** run the specific test you added/modified: `dotnet test --filter "FullyQualifiedName=..."`.
    *   If creating new files, ensure they are added to the `.csproj` (usually automatic in .NET, but verify file location).
    *   Fix any compilation errors immediately.

## 6. Common Patterns (Examples)

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
