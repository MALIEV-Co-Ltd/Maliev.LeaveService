# Implementation Plan: Employee Leave Management Service

**Branch**: `001-leave-service` | **Date**: 2025-12-28 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-leave-service/spec.md`

## Summary

Implementing a standalone microservice for employee leave management, extracted from the existing Employee Service. The service will manage leave requests, balances, approvals, policies, and reporting with complete autonomy. Built on .NET 10 with PostgreSQL, the service integrates with Employee and Notification services via RabbitMQ events, supports multi-level approval workflows, automated monthly accrual, and maintains 7-year audit compliance.

## Technical Context

**Language/Version**: .NET 10.0 (ASP.NET Core 10.0)
**Primary Dependencies**:
- Maliev.Aspire.ServiceDefaults (NuGet package from GitHub Packages)
- Entity Framework Core 10.x
- MassTransit (RabbitMQ integration via ServiceDefaults)
- PostgreSQL driver

**Storage**: PostgreSQL 18 (own database instance)
**Testing**: xUnit with Testcontainers (real PostgreSQL, RabbitMQ, Redis instances)
**Target Platform**: Linux containers (GKE via maliev-gitops)
**Project Type**: Web API microservice (4 projects: Api, Application, Domain, Infrastructure + Tests)
**Performance Goals**:
- Request submission: < 2 minutes end-to-end
- Balance queries: < 5 seconds
- Approval actions: < 1 minute
- Reports: < 10 seconds for 1-year org-wide data
- Monthly accrual: Complete for all employees within 1 hour

**Constraints**:
- Container memory: 128-192MB (dev/staging), 128-192MB (production)
- CPU: 10-40m (dev: 10m request/25m limit, staging/prod: 15m request/40m limit)
- API response time: p95 < 500ms for queries, < 1s for commands
- Database connections: Max 20 pooled
- Audit retention: 7 years minimum

**Scale/Scope**:
- Initial: Single organization/tenant
- Employees: ~1000-10,000 range
- Leave requests: ~50-200 per day
- ~60-70 C# files, ~10,000 LOC

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Service Autonomy ✅ PASS
- **Own Database**: Dedicated PostgreSQL instance (`leave_service` database)
- **Own Schema**: Complete schema ownership (leave_requests, leave_balances, leave_approvals, leave_policies)
- **Event-Based Integration**: Communication via RabbitMQ events (EmployeeCreatedEvent, EmployeeTerminatedEvent)
- **No Direct DB Access**: Employee and Notification services accessed only via HTTP/events

### Explicit Contracts ✅ PASS
- **OpenAPI/Scalar**: Via ServiceDefaults (`app.MapApiDocumentation(servicePrefix: "leave")`)
- **Event Schemas**: MassTransit message contracts versioned
- **Backward Compatibility**: Database migrations with EF Core versioned

### Test-First Development ✅ PASS (Pending Implementation)
- **Real Infrastructure**: Testcontainers for PostgreSQL, RabbitMQ, Redis (NO in-memory providers)
- **Test Categories**: Unit (handlers, validators), Integration (repositories, events), Contract (API, events)
- **Coverage Target**: 80% for business logic
- **Workflow**: Spec → Tests → Implementation → Validation

### Auditability & Observability ✅ PASS
- **Structured Logging**: Via ServiceDefaults with JSON format
- **Audit Logs**: FR-027 mandates 7-year retention for all leave transactions
- **Health Checks**: `/leave/liveness` and `/leave/readiness` via MapDefaultEndpoints
- **Log Level Configuration**: Per constitution Section V

### Security & Compliance ✅ PASS
- **Authentication**: JWT via `builder.AddJwtAuthentication()`
- **Authorization**: Permission-based via `AddPermissionAuthorization()` and LeavePermissions constants
- **Secrets Management**: Google Secret Manager via `builder.AddGoogleSecretManagerVolume()`
- **Data Retention**: 7-year audit log retention for compliance (FR-027)

### Zero Warnings Policy ✅ PASS
- Build configuration enforces `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`

### Clean Project Artifacts ✅ PASS
- **Root Files**: Only README.md, LICENSE, nuget.config, .gitignore, .dockerignore
- **CODEOWNERS**: `.github/CODEOWNERS` with `* @MALIEV-Co-Ltd/core-developers`
- **No Extra Markdown**: No COMPLIANCE_REVIEW.md or similar

### Docker Best Practices ✅ PASS
- **Dockerfile Location**: `Maliev.LeaveService.Api/Dockerfile` (NOT root)
- **User**: Built-in `app` user from Microsoft ASP.NET images
- **BuildKit Secrets**: `--mount=type=secret,id=nuget_username` and `--mount=type=secret,id=nuget_password`
- **Multi-Stage**: SDK build → ASP.NET runtime
- **Health Check**: `HEALTHCHECK CMD curl -f http://localhost:8080/leave/liveness || exit 1`
- **Port**: `EXPOSE 8080` and `ENV ASPNETCORE_URLS=http://+:8080`

### .NET Aspire Integration ✅ PASS
- **ServiceDefaults**: Consumed as NuGet package from GitHub Packages
- **nuget.config**: Present with GitHub Packages source and credential placeholders
- **Program.cs**: Calls `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints(servicePrefix: "leave")`
- **CI/CD Auth**: Uses `GITOPS_PAT` with `read:packages` scope

### Code Quality Standards ✅ PASS
- **NO AutoMapper**: Explicit DTO mapping
- **NO FluentValidation**: Data Annotations for validation
- **NO FluentAssertions**: Standard xUnit Assert

### Project Structure & Naming ✅ PASS
- **Flat Structure**: Projects at repository root (NO /src or /tests folders)
- **Naming**: `Maliev.LeaveService.Api`, `Maliev.LeaveService.Application`, etc.
- **Dockerfile**: Inside `Maliev.LeaveService.Api/` folder

### CI/CD Standards ✅ PASS
- **Workflow Names**: `ci-develop.yml`, `ci-staging.yml`, `ci-main.yml`
- **No Docker Compose**: Testcontainers for all testing

### Business Metrics & Analytics ✅ PASS (Pending Implementation)
- **Service Meters**: `builder.AddServiceMeters("leave-service")`
- **Business Metrics** (to be instrumented):
  - Leave requests submitted per day/week/month
  - Average approval time
  - Leave utilization rate by type
  - Balance expiration events
  - Accrual processing duration

## Project Structure

### Documentation (this feature)

```text
specs/001-leave-service/
├── spec.md              # Feature specification with clarifications
├── plan.md              # This file (implementation plan)
├── research.md          # Phase 0: Research findings (to be generated)
├── data-model.md        # Phase 1: Entity schemas and relationships
├── quickstart.md        # Phase 1: Developer onboarding guide
├── contracts/           # Phase 1: API contracts (OpenAPI schemas, event schemas)
│   ├── api-contracts.yaml
│   └── event-contracts.md
└── tasks.md             # Phase 2: Task breakdown (created by /speckit.tasks)
```

### Source Code (repository root)

```text
.github/
├── workflows/
│   ├── ci-develop.yml
│   ├── ci-staging.yml
│   └── ci-main.yml
└── CODEOWNERS

Maliev.LeaveService.Api/
├── Controllers/
│   ├── LeaveBalancesController.cs
│   ├── LeaveRequestsController.cs
│   ├── LeavePoliciesController.cs
│   └── LeaveReportsController.cs
├── Dockerfile
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── Maliev.LeaveService.Api.csproj

Maliev.LeaveService.Application/
├── Commands/
│   ├── SubmitLeaveRequestCommand.cs
│   ├── ApproveRejectLeaveCommand.cs
│   ├── CancelLeaveRequestCommand.cs
│   ├── CreateLeavePolicyCommand.cs
│   ├── UpdateLeavePolicyCommand.cs
│   └── Handlers/
│       ├── SubmitLeaveRequestCommandHandler.cs
│       ├── ApproveRejectLeaveCommandHandler.cs
│       ├── CancelLeaveRequestCommandHandler.cs
│       ├── CreateLeavePolicyCommandHandler.cs
│       └── UpdateLeavePolicyCommandHandler.cs
├── Queries/
│   ├── GetLeaveBalancesQuery.cs
│   ├── GetLeaveRequestsQuery.cs
│   ├── GetPendingApprovalsQuery.cs
│   ├── GetLeavePoliciesQuery.cs
│   ├── GetUtilizationReportQuery.cs
│   └── Handlers/
│       ├── GetLeaveBalancesQueryHandler.cs
│       ├── GetLeaveRequestsQueryHandler.cs
│       ├── GetPendingApprovalsQueryHandler.cs
│       ├── GetLeavePoliciesQueryHandler.cs
│       └── GetUtilizationReportQueryHandler.cs
├── DTOs/
│   ├── Requests/
│   │   ├── SubmitLeaveRequestDto.cs
│   │   ├── ApproveRejectLeaveDto.cs
│   │   ├── CancelLeaveRequestDto.cs
│   │   ├── CreateLeavePolicyDto.cs
│   │   └── UpdateLeavePolicyDto.cs
│   └── Responses/
│       ├── LeaveBalanceDto.cs
│       ├── LeaveRequestDto.cs
│       ├── LeaveApprovalDto.cs
│       ├── LeavePolicyDto.cs
│       └── UtilizationReportDto.cs
├── Interfaces/
│   ├── ILeaveRequestRepository.cs
│   ├── ILeaveBalanceRepository.cs
│   ├── ILeaveApprovalRepository.cs
│   ├── ILeavePolicyRepository.cs
│   └── INotificationService.cs
└── Maliev.LeaveService.Application.csproj

Maliev.LeaveService.Domain/
├── Entities/
│   ├── LeaveRequest.cs
│   ├── LeaveBalance.cs
│   ├── LeaveApproval.cs
│   └── LeavePolicy.cs
├── Enums/
│   ├── LeaveType.cs
│   ├── LeaveRequestStatus.cs
│   ├── ApprovalStatus.cs
│   └── HalfDayPeriod.cs
├── Events/
│   ├── Published/
│   │   ├── LeaveRequestSubmittedEvent.cs
│   │   ├── LeaveRequestApprovedEvent.cs
│   │   ├── LeaveRequestRejectedEvent.cs
│   │   ├── LeaveRequestCancelledEvent.cs
│   │   └── LeaveBalanceUpdatedEvent.cs
│   └── Consumed/
│       ├── EmployeeCreatedEvent.cs
│       └── EmployeeTerminatedEvent.cs
├── Authorization/
│   └── LeavePermissions.cs
└── Maliev.LeaveService.Domain.csproj

Maliev.LeaveService.Infrastructure/
├── Data/
│   ├── LeaveDbContext.cs
│   ├── Configurations/
│   │   ├── LeaveRequestConfiguration.cs
│   │   ├── LeaveBalanceConfiguration.cs
│   │   ├── LeaveApprovalConfiguration.cs
│   │   └── LeavePolicyConfiguration.cs
│   ├── Migrations/
│   └── SnakeCaseNamingHelper.cs
├── Repositories/
│   ├── LeaveRequestRepository.cs
│   ├── LeaveBalanceRepository.cs
│   ├── LeaveApprovalRepository.cs
│   └── LeavePolicyRepository.cs
├── Services/
│   ├── NotificationService.cs
│   ├── EmployeeServiceClient.cs
│   └── LeaveIAMRegistrationService.cs
├── BackgroundServices/
│   ├── LeaveAccrualBackgroundService.cs
│   └── LeaveExpirationAlertBackgroundService.cs
├── Consumers/
│   ├── EmployeeCreatedEventConsumer.cs
│   └── EmployeeTerminatedEventConsumer.cs
└── Maliev.LeaveService.Infrastructure.csproj

Maliev.LeaveService.Tests/
├── Unit/
│   ├── Handlers/
│   │   ├── SubmitLeaveRequestCommandHandlerTests.cs
│   │   ├── ApproveRejectLeaveCommandHandlerTests.cs
│   │   └── CancelLeaveRequestCommandHandlerTests.cs
│   ├── Validation/
│   │   ├── LeaveRequestValidationTests.cs
│   │   ├── BalanceCalculationTests.cs
│   │   └── OverlapDetectionTests.cs
│   └── Domain/
│       └── LeaveBalanceTests.cs
├── Integration/
│   ├── Repositories/
│   │   ├── LeaveRequestRepositoryTests.cs
│   │   └── LeaveBalanceRepositoryTests.cs
│   ├── Events/
│   │   ├── EmployeeCreatedEventConsumerTests.cs
│   │   └── LeaveRequestApprovedEventTests.cs
│   └── BackgroundServices/
│       ├── LeaveAccrualBackgroundServiceTests.cs
│       └── LeaveExpirationAlertBackgroundServiceTests.cs
├── Contract/
│   ├── Api/
│   │   ├── LeaveRequestsApiContractTests.cs
│   │   └── LeaveBalancesApiContractTests.cs
│   └── Events/
│       └── EventSchemaContractTests.cs
├── TestUtilities/
│   ├── TestContainersFixture.cs
│   ├── TestDataBuilder.cs
│   └── DatabaseSeeder.cs
└── Maliev.LeaveService.Tests.csproj

.dockerignore
.gitignore
nuget.config
README.md
LICENSE
Maliev.LeaveService.sln
```

**Structure Decision**:
- **4 Projects**: Api (controllers, Program.cs), Application (CQRS handlers, DTOs), Domain (entities, events), Infrastructure (repositories, EF Core, MassTransit consumers)
- **Single Tests Project**: All test categories (Unit, Integration, Contract) in one project using Testcontainers
- **Flat Root**: All projects directly at repository root per constitution Section XV
- **Naming**: Full `Maliev.LeaveService.*` prefix per constitution

## Complexity Tracking

**NO VIOLATIONS** - All constitution requirements are met.

This service follows clean architecture (Domain → Application → Infrastructure → Api) which is standard for .NET microservices and aligns with the constitution's simplicity principle. The 4-project structure is justified:

1. **Domain**: Pure business logic and entities (no dependencies)
2. **Application**: Use case orchestration (depends only on Domain)
3. **Infrastructure**: External concerns (depends on Application + Domain)
4. **Api**: HTTP entry point (depends on Application)

This separation enables independent testing, clear dependency flow, and maintainability without violating YAGNI.

## Phase 0: Research Plan

### Research Tasks

1. **EF Core Migrations Strategy**
   - Research: Best practices for zero-downtime PostgreSQL migrations in k8s
   - Decision needed: Migration execution timing (startup vs manual)
   - Output: Migration execution pattern

2. **MassTransit Queue Configuration**
   - Research: Optimal queue topology for leave events
   - Decision needed: Exchange patterns, retry policies, dead-letter handling
   - Output: RabbitMQ topology design

3. **Caching Strategy**
   - Research: Redis caching patterns for read-heavy data (leave policies)
   - Decision needed: Cache invalidation strategy, TTL values
   - Output: Caching implementation pattern

4. **Background Service Scheduling**
   - Research: NCron vs Quartz vs built-in HostedService for monthly accrual
   - Decision needed: Scheduling library selection
   - Output: Scheduler choice and configuration

5. **Employee Service Integration Resilience**
   - Research: Polly patterns for HTTP client resilience (circuit breaker, retry)
   - Decision needed: Timeout values, retry counts, circuit breaker thresholds
   - Output: Resilience policy configuration

6. **Multi-Level Approval State Machine**
   - Research: State transition patterns for complex approval flows
   - Decision needed: State tracking approach (in-memory vs database-driven)
   - Output: Approval workflow engine design

7. **Audit Log Archival**
   - Research: PostgreSQL table partitioning for 7-year retention
   - Decision needed: Partitioning strategy (yearly vs quarterly)
   - Output: Audit table design with partitioning

8. **Leave Request Queueing (Employee Service Unavailability)**
   - Research: Outbox pattern vs eventual consistency strategies
   - Decision needed: Queue implementation for FR-031 (delayed validation)
   - Output: Request queue design

9. **Balance Calculation Consistency**
   - Research: Optimistic vs pessimistic locking for concurrent balance updates
   - Decision needed: Concurrency control approach
   - Output: Transaction isolation strategy

10. **Testcontainers Setup**
    - Research: Best practices for PostgreSQL + RabbitMQ + Redis in xUnit
    - Decision needed: Shared fixture vs per-test containers
    - Output: Test infrastructure pattern

### Expected Outputs

**research.md** will contain:
- Each decision with rationale
- Alternatives considered and rejected
- Code examples for chosen patterns
- Configuration recommendations
- Performance implications

## Phase 1: Design Artifacts

### data-model.md
- **LeaveRequest** entity with fields, relationships, state transitions
- **LeaveBalance** calculation formula and constraints
- **LeaveApproval** workflow states
- **LeavePolicy** configuration schema
- Database indexes and constraints
- Migration strategy

### contracts/
- **api-contracts.yaml**: OpenAPI 3.1 specification
  - All endpoints from spec (balances, requests, policies, reports)
  - Request/response schemas
  - Authentication/authorization requirements
- **event-contracts.md**: MassTransit message contracts
  - Published events schemas (LeaveRequestSubmittedEvent, etc.)
  - Consumed events schemas (EmployeeCreatedEvent, etc.)
  - Versioning strategy

### quickstart.md
- Prerequisites (Docker, .NET 10 SDK)
- Clone and build instructions
- Testcontainers setup
- Running locally with ServiceDefaults
- Sample API calls (curl/Postman collection)
- Database migrations
- Common troubleshooting

## Phase 2: Task Breakdown

**Deferred to `/speckit.tasks` command** - Will generate dependency-ordered implementation tasks based on:
- User stories prioritization (P1 → P2 → P3)
- Technical dependencies (Domain → Application → Infrastructure → Api)
- Test-first workflow (Tests → Implementation → Validation)

## Key Implementation Notes

### Database Schema Highlights
- **snake_case naming**: Via `SnakeCaseNamingHelper.ApplySnakeCaseNaming(modelBuilder)`
- **UTC timestamps**: All datetime columns use `TIMESTAMP WITH TIME ZONE`
- **Decimal precision**: `DECIMAL(5,2)` for leave days (supports up to 999.99 days)
- **Unique constraints**: `(employee_id, leave_type, year)` on leave_balances
- **Cascade deletes**: leave_approvals cascade delete when leave_request deleted

### API Endpoints Summary
- **GET /leave/v1/balances/{employeeId}**: Retrieve leave balances
- **POST /leave/v1/requests/{employeeId}**: Submit leave request
- **PUT /leave/v1/requests/{requestId}/decision**: Approve/reject
- **PUT /leave/v1/requests/{requestId}/cancel**: Cancel request
- **GET /leave/v1/pending-approvals**: Manager approval queue
- **GET /leave/v1/policies**: List leave policies (admin)
- **POST /leave/v1/policies**: Create policy (admin)
- **GET /leave/v1/reports/utilization**: Utilization reporting

### Background Services
1. **LeaveAccrualBackgroundService**: Cron `0 0 1 * *` (monthly on 1st at midnight)
   - Accrue leave for all active employees
   - Handle pro-ration for mid-month joins/terminations
   - Expire carried forward balances past expiration date

2. **LeaveExpirationAlertBackgroundService**: Cron `0 6 * * *` (daily at 6 AM)
   - Find balances expiring within 30 days (per clarification)
   - Publish notification events to Notification Service

### Integration Patterns
- **Employee Service**: HTTP client with Polly resilience (circuit breaker, retry, timeout)
- **Notification Service**: HTTP client for alert delivery
- **Event Publishing**: MassTransit with RabbitMQ for asynchronous event propagation
- **Event Consumption**: MassTransit consumers for EmployeeCreated/Terminated events

### Resource Optimization
- **AsNoTracking()**: All read queries use AsNoTracking for memory efficiency
- **Projection**: Select only required fields in DTOs
- **Redis caching**: Leave policies cached (1-hour TTL)
- **Connection pooling**: Max 20 database connections
- **Memory limits**: 96-192MB depending on environment

### Security & Authorization
- **JWT Authentication**: Via ServiceDefaults `AddJwtAuthentication()`
- **Permission-Based**: LeavePermissions constants (leave.create, leave.read, leave.approve, leave.admin, etc.)
- **Resource Scoping**: Employees can only access their own data, managers access team data, HR access all
- **Audit Logging**: All mutations logged with user ID, action type, timestamp

### Data Migration from Employee Service
- **Export**: SQL COPY commands to CSV
- **Import**: SQL COPY commands from CSV
- **Verification**: Count validation and referential integrity checks
- **Rollback Plan**: Maintain Employee Service leave tables during transition period

### CI/CD Integration
- **GitHub Actions**: ci-develop.yml, ci-staging.yml, ci-main.yml
- **Docker Build**: BuildKit secrets for NuGet credentials
- **GitOps Update**: Push to maliev-gitops repository to trigger ArgoCD deployment
- **Image Registry**: asia-southeast1-docker.pkg.dev/maliev-website

### Testing Strategy
- **Unit Tests**: Handlers, validators, business logic (mocked repositories)
- **Integration Tests**: Repositories, events, background services (Testcontainers with real PostgreSQL/RabbitMQ/Redis)
- **Contract Tests**: API endpoint contracts, event schema validation
- **Coverage Goal**: 80% minimum for business-critical code

## Next Steps

1. **Execute /speckit.plan Phase 0**: Generate research.md
2. **Execute /speckit.plan Phase 1**: Generate data-model.md, contracts/, quickstart.md
3. **Execute /speckit.tasks**: Generate dependency-ordered task breakdown
4. **Execute /speckit.implement**: Begin test-first implementation per tasks.md
