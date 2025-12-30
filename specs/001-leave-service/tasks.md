# Tasks: Employee Leave Management Service

**Input**: Design documents from `/specs/001-leave-service/`
**Prerequisites**: plan.md (required), spec.md (required for user stories)

**Tests**: This feature requires Test-First Development per constitution Section III. All test tasks MUST be completed and verified as FAILING before implementation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Per plan.md, this project uses flat structure at repository root:
- **Projects**: `Maliev.LeaveService.Api/`, `Maliev.LeaveService.Application/`, `Maliev.LeaveService.Domain/`, `Maliev.LeaveService.Infrastructure/`, `Maliev.LeaveService.Tests/`
- **Tests**: `Maliev.LeaveService.Tests/Unit/`, `Maliev.LeaveService.Tests/Integration/`, `Maliev.LeaveService.Tests/Contract/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Create solution file Maliev.LeaveService.sln at repository root
- [x] T002 Create Maliev.LeaveService.Domain project with csproj at Maliev.LeaveService.Domain/Maliev.LeaveService.Domain.csproj
- [x] T003 [P] Create Maliev.LeaveService.Application project with csproj at Maliev.LeaveService.Application/Maliev.LeaveService.Application.csproj
- [x] T004 [P] Create Maliev.LeaveService.Infrastructure project with csproj at Maliev.LeaveService.Infrastructure/Maliev.LeaveService.Infrastructure.csproj
- [x] T005 [P] Create Maliev.LeaveService.Api project with csproj at Maliev.LeaveService.Api/Maliev.LeaveService.Api.csproj
- [x] T006 [P] Create Maliev.LeaveService.Tests project with csproj at Maliev.LeaveService.Tests/Maliev.LeaveService.Tests.csproj
- [x] T007 Add project references (Application → Domain, Infrastructure → Application + Domain, Api → Application, Tests → All)
- [x] T008 [P] Create nuget.config at repository root with GitHub Packages source configuration
- [x] T009 [P] Create .gitignore at repository root with .NET artifacts exclusions
- [x] T010 [P] Create .dockerignore at repository root excluding build outputs, IDE files, specs, Test project
- [x] T011 [P] Create README.md at repository root with project overview and setup instructions
- [x] T012 [P] Create LICENSE file at repository root
- [x] T013 Create .github/CODEOWNERS with content: * @MALIEV-Co-Ltd/core-developers
- [x] T014 [P] Create .github/workflows/ci-develop.yml for develop branch CI/CD
- [x] T015 [P] Create .github/workflows/ci-staging.yml for staging branch CI/CD
- [x] T016 [P] Create .github/workflows/ci-main.yml for main branch CI/CD
- [x] T017 Add Maliev.Aspire.ServiceDefaults NuGet package reference to Api project
- [x] T018 [P] Add Entity Framework Core 10.x packages to Infrastructure project
- [x] T019 [P] Add MassTransit packages to Infrastructure project (via ServiceDefaults)
- [x] T020 [P] Add xUnit and Testcontainers packages to Tests project
- [x] T021 Configure all projects with TreatWarningsAsErrors=true and .NET 10 target framework

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T022 Create LeaveType enum in Maliev.LeaveService.Domain/Enums/LeaveType.cs (Annual, Sick, Personal, Maternity, Paternity, Unpaid, Bereavement, Study)
- [x] T023 [P] Create LeaveRequestStatus enum in Maliev.LeaveService.Domain/Enums/LeaveRequestStatus.cs (Pending, Approved, Rejected, Cancelled, PartiallyApproved)
- [x] T024 [P] Create ApprovalStatus enum in Maliev.LeaveService.Domain/Enums/ApprovalStatus.cs (Pending, Approved, Rejected)
- [x] T025 [P] Create HalfDayPeriod enum in Maliev.LeaveService.Domain/Enums/HalfDayPeriod.cs (FullDay, Morning, Afternoon)
- [x] T026 Create LeavePermissions constants class in Maliev.LeaveService.Domain/Authorization/LeavePermissions.cs (leave.create, leave.read, leave.approve, leave.cancel, leave.admin, leave.reports)
- [x] T027 Create LeaveRequest entity in Maliev.LeaveService.Domain/Entities/LeaveRequest.cs with properties per data model
- [x] T028 [P] Create LeaveBalance entity in Maliev.LeaveService.Domain/Entities/LeaveBalance.cs with Available calculated property
- [x] T029 [P] Create LeaveApproval entity in Maliev.LeaveService.Domain/Entities/LeaveApproval.cs with navigation to LeaveRequest
- [x] T030 [P] Create LeavePolicy entity in Maliev.LeaveService.Domain/Entities/LeavePolicy.cs with policy configuration fields
- [x] T031 Create LeaveDbContext in Maliev.LeaveService.Infrastructure/Data/LeaveDbContext.cs with DbSet properties
- [x] T032 Create SnakeCaseNamingHelper in Maliev.LeaveService.Infrastructure/Data/SnakeCaseNamingHelper.cs for PostgreSQL naming convention
- [x] T033 [P] Create LeaveRequestConfiguration in Maliev.LeaveService.Infrastructure/Data/Configurations/LeaveRequestConfiguration.cs for EF Core entity configuration
- [x] T034 [P] Create LeaveBalanceConfiguration in Maliev.LeaveService.Infrastructure/Data/Configurations/LeaveBalanceConfiguration.cs with unique constraint on (employee_id, leave_type, year)
- [x] T035 [P] Create LeaveApprovalConfiguration in Maliev.LeaveService.Infrastructure/Data/Configurations/LeaveApprovalConfiguration.cs with foreign key cascade
- [x] T036 [P] Create LeavePolicyConfiguration in Maliev.LeaveService.Infrastructure/Data/Configurations/LeavePolicyConfiguration.cs with unique constraint on leave_type
- [x] T037 Apply snake_case naming convention in LeaveDbContext.OnModelCreating using SnakeCaseNamingHelper
- [x] T038 Apply all entity configurations in LeaveDbContext.OnModelCreating
- [x] T039 Create initial EF Core migration for database schema in Maliev.LeaveService.Infrastructure/Data/Migrations/
- [x] T040 Create Program.cs in Maliev.LeaveService.Api/ with ServiceDefaults integration (AddServiceDefaults, AddGoogleSecretManagerVolume, AddStandardMiddleware)
- [x] T041 Add PostgreSQL DbContext registration in Program.cs using builder.AddPostgresDbContext<LeaveDbContext>
- [x] T042 [P] Add Redis distributed cache in Program.cs using builder.AddRedisDistributedCache(instanceName: "leave:")
- [x] T043 [P] Add MassTransit with RabbitMQ in Program.cs using builder.AddMassTransitWithRabbitMq
- [x] T044 [P] Add JWT authentication in Program.cs using builder.AddJwtAuthentication()
- [x] T045 [P] Add permission authorization in Program.cs using builder.Services.AddPermissionAuthorization()
- [x] T046 [P] Add CORS configuration in Program.cs using builder.AddDefaultCors()
- [x] T047 [P] Add API versioning in Program.cs using builder.AddDefaultApiVersioning()
- [x] T048 [P] Add rate limiting in Program.cs using builder.AddStandardRateLimiting()
- [x] T049 [P] Add service meters in Program.cs using builder.AddServiceMeters("leave-service")
- [x] T050 Configure middleware pipeline in Program.cs (UseStandardMiddleware, UseHttpsRedirection, UseCors, UseAuthentication, UseAuthorization, UseRateLimiter)
- [x] T051 Map controllers and endpoints in Program.cs (MapControllers, MapDefaultEndpoints(servicePrefix: "leave"), MapApiDocumentation(servicePrefix: "leave"))
- [x] T052 Add database migration execution in Program.cs using app.MigrateDatabaseAsync<LeaveDbContext>()
- [x] T053 Create appsettings.json in Maliev.LeaveService.Api/ with connection strings and external service URLs
- [x] T054 [P] Create appsettings.Development.json in Maliev.LeaveService.Api/ with local development settings
- [x] T055 Add LogLevel configuration to appsettings.json per constitution Section V
- [x] T056 Create Dockerfile in Maliev.LeaveService.Api/Dockerfile using multi-stage build with BuildKit secrets
- [x] T057 Add health check configuration to Dockerfile (HEALTHCHECK CMD curl -f http://localhost:8080/leave/liveness || exit 1)
- [x] T058 [P] Create TestContainersFixture.cs in Maliev.LeaveService.Tests/TestUtilities/ for PostgreSQL, RabbitMQ, Redis containers
- [x] T059 [P] Create TestDataBuilder.cs in Maliev.LeaveService.Tests/TestUtilities/ for test data generation
- [x] T060 [P] Create DatabaseSeeder.cs in Maliev.LeaveService.Tests/TestUtilities/ for test database seeding
- [x] T061 Create ILeaveRequestRepository interface in Maliev.LeaveService.Application/Interfaces/ILeaveRequestRepository.cs
- [x] T062 [P] Create ILeaveBalanceRepository interface in Maliev.LeaveService.Application/Interfaces/ILeaveBalanceRepository.cs
- [x] T063 [P] Create ILeaveApprovalRepository interface in Maliev.LeaveService.Application/Interfaces/ILeaveApprovalRepository.cs
- [x] T064 [P] Create ILeavePolicyRepository interface in Maliev.LeaveService.Application/Interfaces/ILeavePolicyRepository.cs
- [x] T065 [P] Create INotificationService interface in Maliev.LeaveService.Application/Interfaces/INotificationService.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Employee Submits Leave Request (Priority: P1) 🎯 MVP

**Goal**: Enable employees to submit leave requests with validation for balance, dates, overlaps, and advance notice requirements. Queue requests for delayed validation when Employee Service is unavailable.

**Independent Test**: Create an employee account, login, view leave balances, submit a leave request with valid dates at least 24 hours in advance, and verify the request appears in pending status with balance updated.

### Tests for User Story 1 (Test-First Required) ⚠️

> **CRITICAL: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T066 [P] [US1] Contract test for GET /leave/v1/balances/{employeeId} endpoint in Maliev.LeaveService.Tests/Contract/Api/LeaveBalancesApiContractTests.cs
- [x] T067 [P] [US1] Contract test for POST /leave/v1/requests/{employeeId} endpoint in Maliev.LeaveService.Tests/Contract/Api/LeaveRequestsApiContractTests.cs
- [x] T068 [P] [US1] Integration test for leave request submission with sufficient balance in Maliev.LeaveService.Tests/Integration/LeaveRequestSubmissionTests.cs
- [x] T069 [P] [US1] Integration test for leave request rejection with insufficient balance in Maliev.LeaveService.Tests/Integration/LeaveRequestSubmissionTests.cs
- [x] T070 [P] [US1] Integration test for overlapping request detection in Maliev.LeaveService.Tests/Integration/LeaveRequestSubmissionTests.cs
- [x] T071 [P] [US1] Integration test for half-day leave request (morning/afternoon) in Maliev.LeaveService.Tests/Integration/LeaveRequestSubmissionTests.cs
- [x] T072 [P] [US1] Integration test for date validation (start date after end date, duration > 30 days) in Maliev.LeaveService.Tests/Integration/LeaveRequestSubmissionTests.cs
- [x] T073 [P] [US1] Integration test for 24-hour advance notice validation in Maliev.LeaveService.Tests/Integration/LeaveRequestSubmissionTests.cs
- [x] T074 [P] [US1] Integration test for Employee Service unavailability (queue for delayed validation) in Maliev.LeaveService.Tests/Integration/LeaveRequestSubmissionTests.cs
- [x] T075 [P] [US1] Unit test for balance calculation logic in Maliev.LeaveService.Tests/Unit/Domain/LeaveBalanceTests.cs
- [x] T076 [P] [US1] Unit test for overlap detection logic in Maliev.LeaveService.Tests/Unit/Validation/OverlapDetectionTests.cs
- [x] T077 [P] [US1] Unit test for SubmitLeaveRequestCommandHandler in Maliev.LeaveService.Tests/Unit/Handlers/SubmitLeaveRequestCommandHandlerTests.cs

### Implementation for User Story 1

- [x] T078 [P] [US1] Create SubmitLeaveRequestDto in Maliev.LeaveService.Application/DTOs/Requests/SubmitLeaveRequestDto.cs with Data Annotations validation
- [x] T079 [P] [US1] Create LeaveBalanceDto in Maliev.LeaveService.Application/DTOs/Responses/LeaveBalanceDto.cs
- [x] T080 [P] [US1] Create LeaveRequestDto in Maliev.LeaveService.Application/DTOs/Responses/LeaveRequestDto.cs
- [x] T081 [US1] Create SubmitLeaveRequestCommand in Maliev.LeaveService.Application/Commands/SubmitLeaveRequestCommand.cs
- [x] T082 [US1] Create GetLeaveBalancesQuery in Maliev.LeaveService.Application/Queries/GetLeaveBalancesQuery.cs
- [x] T083 [US1] Create LeaveRequestRepository in Maliev.LeaveService.Infrastructure/Repositories/LeaveRequestRepository.cs implementing ILeaveRequestRepository
- [x] T084 [P] [US1] Create LeaveBalanceRepository in Maliev.LeaveService.Infrastructure/Repositories/LeaveBalanceRepository.cs implementing ILeaveBalanceRepository
- [x] T085 [US1] Create SubmitLeaveRequestCommandHandler in Maliev.LeaveService.Application/Commands/Handlers/SubmitLeaveRequestCommandHandler.cs with all validation logic:
  - Validate dates (start not after end, duration ≤ 30 days, 24-hour advance notice for non-sick leave)
  - Check sufficient balance
  - Detect overlapping requests (except half-day AM + PM same day)
  - Update pending balance
  - Queue for delayed validation if Employee Service unavailable (FR-031)
- [x] T086 [US1] Create GetLeaveBalancesQueryHandler in Maliev.LeaveService.Application/Queries/Handlers/GetLeaveBalancesQueryHandler.cs with projection and AsNoTracking()
- [x] T087 [US1] Create LeaveRequestsController in Maliev.LeaveService.Api/Controllers/LeaveRequestsController.cs with POST /leave/v1/requests/{employeeId} endpoint
- [x] T088 [P] [US1] Create LeaveBalancesController in Maliev.LeaveService.Api/Controllers/LeaveBalancesController.cs with GET /leave/v1/balances/{employeeId} endpoint
- [x] T089 [US1] Add permission authorization to LeaveRequestsController using [RequirePermission(LeavePermissions.Create)]
- [x] T090 [P] [US1] Add permission authorization to LeaveBalancesController using [RequirePermission(LeavePermissions.Read)]
- [x] T091 [US1] Register SubmitLeaveRequestCommandHandler in Program.cs using builder.Services.AddScoped
- [x] T092 [P] [US1] Register GetLeaveBalancesQueryHandler in Program.cs using builder.Services.AddScoped
- [x] T093 [P] [US1] Register LeaveRequestRepository in Program.cs using builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>
- [x] T094 [P] [US1] Register LeaveBalanceRepository in Program.cs using builder.Services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>
- [x] T095 [US1] Create LeaveRequestSubmittedEvent in Maliev.LeaveService.Domain/Events/Published/LeaveRequestSubmittedEvent.cs
- [x] T096 [US1] Publish LeaveRequestSubmittedEvent in SubmitLeaveRequestCommandHandler after successful submission
- [ ] T097 [US1] Add logging for leave request submission in SubmitLeaveRequestCommandHandler
- [ ] T098 [US1] Add audit logging for leave request creation (FR-027 compliance)

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently. Employee can view balances and submit leave requests with full validation.

---

## Phase 4: User Story 2 - Manager Approves or Rejects Leave Requests (Priority: P1)

**Goal**: Enable managers to view pending leave requests from their team and approve/reject them with comments. Support multi-level approval workflows and HR override capabilities.

**Independent Test**: Login as a manager, view pending approval queue with employee name/dates/reason, approve a request with comments, verify employee is notified and balance is updated.

### Tests for User Story 2 (Test-First Required) ⚠️

> **CRITICAL: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T099 [P] [US2] Contract test for GET /leave/v1/pending-approvals endpoint in Maliev.LeaveService.Tests/Contract/Api/LeaveApprovalsApiContractTests.cs
- [x] T100 [P] [US2] Contract test for PUT /leave/v1/requests/{requestId}/decision endpoint in Maliev.LeaveService.Tests/Contract/Api/LeaveApprovalsApiContractTests.cs
- [x] T101 [P] [US2] Integration test for manager viewing pending approvals in Maliev.LeaveService.Tests/Integration/LeaveApprovalTests.cs
- [x] T102 [P] [US2] Integration test for approve request with notification in Maliev.LeaveService.Tests/Integration/LeaveApprovalTests.cs
- [x] T103 [P] [US2] Integration test for reject request with pending days returned to balance in Maliev.LeaveService.Tests/Integration/LeaveApprovalTests.cs
- [x] T104 [P] [US2] Integration test for multi-level approval workflow in Maliev.LeaveService.Tests/Integration/LeaveApprovalTests.cs
- [x] T105 [P] [US2] Integration test for HR override approval in Maliev.LeaveService.Tests/Integration/LeaveApprovalTests.cs
- [x] T106 [P] [US2] Unit test for ApproveRejectLeaveCommandHandler in Maliev.LeaveService.Tests/Unit/Handlers/ApproveRejectLeaveCommandHandlerTests.cs

### Implementation for User Story 2

- [x] T107 [P] [US2] Create ApproveRejectLeaveDto in Maliev.LeaveService.Application/DTOs/Requests/ApproveRejectLeaveDto.cs with Data Annotations
- [x] T108 [P] [US2] Create LeaveApprovalDto in Maliev.LeaveService.Application/DTOs/Responses/LeaveApprovalDto.cs
- [x] T109 [US2] Create ApproveRejectLeaveCommand in Maliev.LeaveService.Application/Commands/ApproveRejectLeaveCommand.cs
- [x] T110 [US2] Create GetPendingApprovalsQuery in Maliev.LeaveService.Application/Queries/GetPendingApprovalsQuery.cs
- [x] T111 [US2] Create LeaveApprovalRepository in Maliev.LeaveService.Infrastructure/Repositories/LeaveApprovalRepository.cs implementing ILeaveApprovalRepository
- [x] T112 [US2] Create ApproveRejectLeaveCommandHandler in Maliev.LeaveService.Application/Commands/Handlers/ApproveRejectLeaveCommandHandler.cs with logic:
  - Validate approver has permission (manager of employee or HR)
  - Update leave request status based on approval decision
  - For approve: move to next approval level if multi-level, or mark approved if final
  - For reject: return pending days to available balance
  - Send notification to employee
- [x] T113 [US2] Create GetPendingApprovalsQueryHandler in Maliev.LeaveService.Application/Queries/Handlers/GetPendingApprovalsQueryHandler.cs filtering by approver
- [x] T114 [US2] Add PUT /leave/v1/requests/{requestId}/decision endpoint to LeaveRequestsController
- [x] T115 [P] [US2] Add GET /leave/v1/pending-approvals endpoint to LeaveRequestsController
- [x] T116 [US2] Add permission authorization using [RequirePermission(LeavePermissions.Approve)]
- [x] T117 [US2] Register ApproveRejectLeaveCommandHandler in Program.cs
- [x] T118 [P] [US2] Register GetPendingApprovalsQueryHandler in Program.cs
- [x] T119 [P] [US2] Register LeaveApprovalRepository in Program.cs
- [x] T120 [US2] Create LeaveRequestApprovedEvent in Maliev.LeaveService.Domain/Events/Published/LeaveRequestApprovedEvent.cs
- [x] T121 [P] [US2] Create LeaveRequestRejectedEvent in Maliev.LeaveService.Domain/Events/Published/LeaveRequestRejectedEvent.cs
- [x] T122 [US2] Create NotificationService in Maliev.LeaveService.Infrastructure/Services/NotificationService.cs implementing INotificationService with HTTP client to Notification Service
- [x] T123 [US2] Add HTTP client for NotificationService in Program.cs with AddStandardResilienceHandler
- [x] T124 [US2] Register NotificationService in Program.cs
- [x] T125 [US2] Publish LeaveRequestApprovedEvent or LeaveRequestRejectedEvent in ApproveRejectLeaveCommandHandler
- [x] T126 [US2] Call NotificationService to send notification in ApproveRejectLeaveCommandHandler
- [ ] T127 [US2] Add logging for approval/rejection decisions
- [ ] T128 [US2] Add audit logging for approval decisions (FR-027 compliance)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently. Complete leave request submission and approval workflow functional.

---

## Phase 5: User Story 3 - Employee Views Leave Balance and History (Priority: P2)

**Goal**: Enable employees to view their leave balances (entitled, used, pending, available, carried forward with expiration) and complete leave request history for planning purposes.

**Independent Test**: Login as employee, view leave balance dashboard showing all leave types with entitled/used/pending/available days, carried forward days with expiration warning, and historical requests with dates/status.

### Tests for User Story 3 (Test-First Required) ⚠️

> **CRITICAL: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T129 [P] [US3] Contract test for GET /leave/v1/balances/{employeeId}?year={year} endpoint in Maliev.LeaveService.Tests/Contract/Api/LeaveBalancesApiContractTests.cs
- [x] T130 [P] [US3] Contract test for GET /leave/v1/requests/{employeeId} endpoint in Maliev.LeaveService.Tests/Contract/Api/LeaveRequestsApiContractTests.cs
- [x] T131 [P] [US3] Integration test for viewing balance with carried forward days and expiration warning in Maliev.LeaveService.Tests/Integration/LeaveBalanceViewTests.cs
- [x] T132 [P] [US3] Integration test for viewing leave history with all statuses in Maliev.LeaveService.Tests/Integration/LeaveBalanceViewTests.cs
- [x] T133 [P] [US3] Integration test for viewing historical balance data for specific year in Maliev.LeaveService.Tests/Integration/LeaveBalanceViewTests.cs
- [x] T134 [P] [US3] Integration test for expiration alert when balance expires in 30 days in Maliev.LeaveService.Tests/Integration/BackgroundServices/LeaveExpirationAlertBackgroundServiceTests.cs
- [x] T135 [P] [US3] Unit test for GetLeaveRequestsQueryHandler in Maliev.LeaveService.Tests/Unit/Handlers/GetLeaveRequestsQueryHandlerTests.cs

### Implementation for User Story 3

- [x] T136 [US3] Create GetLeaveRequestsQuery in Maliev.LeaveService.Application/Queries/GetLeaveRequestsQuery.cs with optional year filter
- [x] T137 [US3] Create GetLeaveRequestsQueryHandler in Maliev.LeaveService.Application/Queries/Handlers/GetLeaveRequestsQueryHandler.cs with filtering by employee and year
- [x] T138 [US3] Update GetLeaveBalancesQueryHandler to support year parameter and include carried forward expiration calculation
- [x] T139 [US3] Add GET /leave/v1/requests/{employeeId} endpoint to LeaveRequestsController
- [x] T140 [US3] Update GET /leave/v1/balances/{employeeId} endpoint to support optional year query parameter
- [x] T141 [US3] Add permission check to ensure employees can only view their own data (resource-scoped authorization)
- [x] T142 [US3] Register GetLeaveRequestsQueryHandler in Program.cs
- [x] T143 [US3] Create LeaveExpirationAlertBackgroundService in Maliev.LeaveService.Infrastructure/BackgroundServices/LeaveExpirationAlertBackgroundService.cs with Cron schedule "0 6 * * *"
- [x] T144 [US3] Implement logic in LeaveExpirationAlertBackgroundService to find balances expiring within 30 days and publish notification events
- [x] T145 [US3] Register LeaveExpirationAlertBackgroundService in Program.cs using builder.Services.AddHostedService
- [x] T146 [US3] Add cron configuration to appsettings.json for LeaveExpirationAlert (DaysBeforeExpiration: 30)
- [x] T147 [US3] Add logging for expiration alert processing

**Checkpoint**: All P1 + P2 priority stories complete. Employees can view balances, request leave, managers can approve, and all data is visible.

---

## Phase 6: User Story 4 - Employee Cancels Leave Request (Priority: P2)

**Goal**: Enable employees to cancel pending or approved leave requests, returning days to available balance, with manager notification for approved requests.

**Independent Test**: Submit a leave request, then cancel it with a reason, verify request status changes to cancelled and days are returned to available balance. For approved request, verify manager notification is sent.

### Tests for User Story 4 (Test-First Required) ⚠️

> **CRITICAL: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T148 [P] [US4] Contract test for PUT /leave/v1/requests/{requestId}/cancel endpoint in Maliev.LeaveService.Tests/Contract/Api/LeaveCancellationApiContractTests.cs
- [x] T149 [P] [US4] Integration test for canceling pending request in Maliev.LeaveService.Tests/Integration/LeaveCancellationTests.cs
- [x] T150 [P] [US4] Integration test for canceling approved request with manager notification in Maliev.LeaveService.Tests/Integration/LeaveCancellationTests.cs
- [x] T151 [P] [US4] Integration test for canceling started request requiring manager confirmation in Maliev.LeaveService.Tests/Integration/LeaveCancellationTests.cs
- [x] T152 [P] [US4] Unit test for CancelLeaveRequestCommandHandler in Maliev.LeaveService.Tests/Unit/Handlers/CancelLeaveRequestCommandHandlerTests.cs

### Implementation for User Story 4

- [x] T153 [P] [US4] Create CancelLeaveRequestDto in Maliev.LeaveService.Application/DTOs/Requests/CancelLeaveRequestDto.cs with optional reason
- [x] T154 [US4] Create CancelLeaveRequestCommand in Maliev.LeaveService.Application/Commands/CancelLeaveRequestCommand.cs
- [x] T155 [US4] Create CancelLeaveRequestCommandHandler in Maliev.LeaveService.Application/Commands/Handlers/CancelLeaveRequestCommandHandler.cs with logic:
  - Update request status to cancelled
  - Return pending/used days to available balance
  - Send notification to manager if request was approved
  - Validate cancellation permissions (employee can cancel own request or manager can confirm)
- [x] T156 [US4] Add PUT /leave/v1/requests/{requestId}/cancel endpoint to LeaveRequestsController
- [x] T157 [US4] Add permission authorization using [RequirePermission(LeavePermissions.Cancel)]
- [x] T158 [US4] Register CancelLeaveRequestCommandHandler in Program.cs
- [x] T159 [US4] Create LeaveRequestCancelledEvent in Maliev.LeaveService.Domain/Events/Published/LeaveRequestCancelledEvent.cs
- [x] T160 [US4] Publish LeaveRequestCancelledEvent in CancelLeaveRequestCommandHandler
- [ ] T161 [US4] Add logging for cancellation operations
- [ ] T162 [US4] Add audit logging for cancellations (FR-027 compliance)

**Checkpoint**: Complete leave request lifecycle now functional: submit → approve/reject → cancel (if needed).

---

## Phase 7: User Story 5 - HR Administers Leave Policies (Priority: P3)

**Goal**: Enable HR administrators to create, update, and configure leave policies defining entitlements, accrual rates, carry-forward rules, and approval requirements for each leave type.

**Independent Test**: Login as HR admin, create a new leave policy with entitlement and accrual settings, verify policy is active and applies to employee entitlements.

### Tests for User Story 5 (Test-First Required) ⚠️

> **CRITICAL: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T163 [P] [US5] Contract test for GET /leave/v1/policies endpoint in Maliev.LeaveService.Tests/Contract/Api/LeavePoliciesApiContractTests.cs
- [x] T164 [P] [US5] Contract test for GET /leave/v1/policies/{id} endpoint in Maliev.LeaveService.Tests/Contract/Api/LeavePoliciesApiContractTests.cs
- [x] T165 [P] [US5] Contract test for POST /leave/v1/policies endpoint in Maliev.LeaveService.Tests/Contract/Api/LeavePoliciesApiContractTests.cs
- [x] T166 [P] [US5] Contract test for PUT /leave/v1/policies/{id} endpoint in Maliev.LeaveService.Tests/Contract/Api/LeavePoliciesApiContractTests.cs
- [x] T167 [P] [US5] Integration test for creating leave policy in Maliev.LeaveService.Tests/Integration/LeavePolicyAdminTests.cs
- [x] T168 [P] [US5] Integration test for updating policy with carry-forward rules in Maliev.LeaveService.Tests/Integration/LeavePolicyAdminTests.cs
- [x] T169 [P] [US5] Integration test for multi-level approval configuration in Maliev.LeaveService.Tests/Integration/LeavePolicyAdminTests.cs
- [x] T170 [P] [US5] Integration test for deactivating policy while preserving historical data in Maliev.LeaveService.Tests/Integration/LeavePolicyAdminTests.cs
- [x] T171 [P] [US5] Unit test for CreateLeavePolicyCommandHandler in Maliev.LeaveService.Tests/Unit/Handlers/CreateLeavePolicyCommandHandlerTests.cs
- [x] T172 [P] [US5] Unit test for UpdateLeavePolicyCommandHandler in Maliev.LeaveService.Tests/Unit/Handlers/UpdateLeavePolicyCommandHandlerTests.cs

### Implementation for User Story 5

- [x] T173 [P] [US5] Create CreateLeavePolicyDto in Maliev.LeaveService.Application/DTOs/Requests/CreateLeavePolicyDto.cs with validation
- [x] T174 [P] [US5] Create UpdateLeavePolicyDto in Maliev.LeaveService.Application/DTOs/Requests/UpdateLeavePolicyDto.cs with validation
- [x] T175 [P] [US5] Create LeavePolicyDto in Maliev.LeaveService.Application/DTOs/Responses/LeavePolicyDto.cs
- [x] T176 [US5] Create CreateLeavePolicyCommand in Maliev.LeaveService.Application/Commands/CreateLeavePolicyCommand.cs
- [x] T177 [P] [US5] Create UpdateLeavePolicyCommand in Maliev.LeaveService.Application/Commands/UpdateLeavePolicyCommand.cs
- [x] T178 [P] [US5] Create GetLeavePoliciesQuery in Maliev.LeaveService.Application/Queries/GetLeavePoliciesQuery.cs
- [x] T179 [US5] Create LeavePolicyRepository in Maliev.LeaveService.Infrastructure/Repositories/LeavePolicyRepository.cs implementing ILeavePolicyRepository with Redis caching
- [ ] T180 [US5] Implement Redis caching in LeavePolicyRepository.GetByTypeAsync with 1-hour TTL
- [x] T181 [US5] Create CreateLeavePolicyCommandHandler in Maliev.LeaveService.Application/Commands/Handlers/CreateLeavePolicyCommandHandler.cs
- [x] T182 [P] [US5] Create UpdateLeavePolicyCommandHandler in Maliev.LeaveService.Application/Commands/Handlers/UpdateLeavePolicyCommandHandler.cs with cache invalidation
- [x] T183 [P] [US5] Create GetLeavePoliciesQueryHandler in Maliev.LeaveService.Application/Queries/Handlers/GetLeavePoliciesQueryHandler.cs
- [x] T184 [US5] Create LeavePoliciesController in Maliev.LeaveService.Api/Controllers/LeavePoliciesController.cs with GET /leave/v1/policies
- [ ] T185 [US5] Add GET /leave/v1/policies/{id} endpoint to LeavePoliciesController
- [x] T186 [P] [US5] Add POST /leave/v1/policies endpoint to LeavePoliciesController
- [x] T187 [P] [US5] Add PUT /leave/v1/policies/{id} endpoint to LeavePoliciesController
- [x] T188 [US5] Add permission authorization using [RequirePermission(LeavePermissions.Admin)]
- [x] T189 [US5] Register all policy handlers in Program.cs
- [x] T190 [P] [US5] Register LeavePolicyRepository in Program.cs
- [ ] T191 [US5] Add logging for policy administration operations
- [ ] T192 [US5] Add audit logging for policy changes (FR-027 compliance)

**Checkpoint**: HR can now fully configure leave policies. System is operationally complete for policy management.

---

## Phase 8: User Story 6 - System Automatically Accrues Leave Balances (Priority: P3)

**Goal**: Implement automated monthly leave accrual on the 1st of each month, handling pro-ration for mid-month joins/terminations and expiring carried forward balances.

**Independent Test**: Configure an accrual policy, simulate month-end processing (manually trigger or wait for schedule), verify all active employees receive monthly accrued leave days with correct pro-ration.

### Tests for User Story 6 (Test-First Required) ⚠️

> **CRITICAL: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T193 [P] [US6] Integration test for monthly accrual processing for all employees in Maliev.LeaveService.Tests/Integration/BackgroundServices/LeaveAccrualBackgroundServiceTests.cs
- [x] T194 [P] [US6] Integration test for pro-rated accrual for mid-month new hire in Maliev.LeaveService.Tests/Integration/BackgroundServices/LeaveAccrualBackgroundServiceTests.cs
- [x] T195 [P] [US6] Integration test for pro-rated accrual for mid-month termination in Maliev.LeaveService.Tests/Integration/BackgroundServices/LeaveAccrualBackgroundServiceTests.cs
- [x] T196 [P] [US6] Integration test for expiring carried forward balances past expiration date in Maliev.LeaveService.Tests/Integration/BackgroundServices/LeaveAccrualBackgroundServiceTests.cs
- [x] T197 [P] [US6] Integration test for EmployeeCreatedEvent consumer in Maliev.LeaveService.Tests/Integration/Events/EmployeeCreatedEventConsumerTests.cs
- [x] T198 [P] [US6] Integration test for EmployeeTerminatedEvent consumer in Maliev.LeaveService.Tests/Integration/Events/EmployeeTerminatedEventConsumerTests.cs

### Implementation for User Story 6

- [x] T199 [US6] Create EmployeeCreatedEvent in Maliev.LeaveService.Domain/Events/Consumed/EmployeeCreatedEvent.cs (per MessagingContracts) with properties: EmployeeId, EmployeeNumber, StartDate, DepartmentId, PositionId?, ManagerId?
- [x] T200 [P] [US6] Create EmployeeTerminatedEvent in Maliev.LeaveService.Domain/Events/Consumed/EmployeeTerminatedEvent.cs (per MessagingContracts) with properties: EmployeeId, TerminationDate, TerminationReason?, EligibleForRehire
- [x] T201 [US6] Create EmployeeCreatedEventConsumer in Maliev.LeaveService.Infrastructure/Consumers/EmployeeCreatedEventConsumer.cs to initialize leave balances for new employee
- [x] T202 [P] [US6] Create EmployeeTerminatedEventConsumer in Maliev.LeaveService.Infrastructure/Consumers/EmployeeTerminatedEventConsumer.cs to settle balances for terminated employee
- [x] T203 [US6] Register consumers in Program.cs MassTransit configuration (x.AddConsumer<EmployeeCreatedEventConsumer>(), x.AddConsumer<EmployeeTerminatedEventConsumer>())
- [x] T204 [US6] Create LeaveAccrualBackgroundService in Maliev.LeaveService.Infrastructure/BackgroundServices/LeaveAccrualBackgroundService.cs with Cron schedule "0 0 1 * *"
- [x] T205 [US6] Implement accrual logic in LeaveAccrualBackgroundService:
  - Fetch all active employees
  - Fetch leave policies
  - Calculate monthly accrual based on policy accrual rate
  - Handle pro-ration for partial month (join/termination dates)
  - Update leave balances
  - Expire carried forward balances past expiration date
- [x] T206 [US6] Create EmployeeServiceClient in Maliev.LeaveService.Infrastructure/Services/EmployeeServiceClient.cs with HTTP client to Employee Service
- [x] T207 [US6] Add HTTP client for EmployeeService in Program.cs with AddStandardResilienceHandler (circuit breaker, retry, timeout)
- [x] T208 [US6] Register EmployeeServiceClient in Program.cs
- [x] T209 [US6] Register LeaveAccrualBackgroundService in Program.cs using builder.Services.AddHostedService
- [x] T210 [US6] Add cron configuration to appsettings.json for LeaveAccrual
- [x] T211 [US6] Create LeaveBalanceUpdatedEvent in Maliev.LeaveService.Domain/Events/Published/LeaveBalanceUpdatedEvent.cs
- [x] T212 [US6] Publish LeaveBalanceUpdatedEvent in LeaveAccrualBackgroundService after balance updates
- [x] T213 [US6] Add logging for accrual processing with metrics (total employees processed, duration)
- [ ] T214 [US6] Add audit logging for balance updates (FR-027 compliance)

**Checkpoint**: Automated accrual now functional. System can operate autonomously for monthly leave accrual.

---

## Phase 9: User Story 7 - HR and Managers View Utilization Reports (Priority: P3)

**Goal**: Enable HR and managers to view leave utilization reports showing usage patterns by department, leave type, and time period for coverage planning and trend analysis.

**Independent Test**: Login as HR admin, generate organization-wide utilization report, verify it shows total days taken by type, average utilization, and peak periods. Manager can filter by department.

### Tests for User Story 7 (Test-First Required) ⚠️

> **CRITICAL: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T215 [P] [US7] Contract test for GET /leave/v1/reports/utilization endpoint in Maliev.LeaveService.Tests/Contract/Api/LeaveReportsApiContractTests.cs
- [x] T216 [P] [US7] Contract test for GET /leave/v1/reports/utilization?departmentId={id} endpoint in Maliev.LeaveService.Tests/Contract/Api/LeaveReportsApiContractTests.cs
- [x] T217 [P] [US7] Integration test for organization-wide utilization report in Maliev.LeaveService.Tests/Integration/LeaveUtilizationReportTests.cs
- [x] T218 [P] [US7] Integration test for department-filtered utilization report in Maliev.LeaveService.Tests/Integration/LeaveUtilizationReportTests.cs
- [x] T219 [P] [US7] Integration test for historical utilization data in Maliev.LeaveService.Tests/Integration/LeaveUtilizationReportTests.cs
- [x] T220 [P] [US7] Unit test for GetUtilizationReportQueryHandler in Maliev.LeaveService.Tests/Unit/Handlers/GetUtilizationReportQueryHandlerTests.cs

### Implementation for User Story 7

- [x] T221 [P] [US7] Create UtilizationReportDto in Maliev.LeaveService.Application/DTOs/Responses/UtilizationReportDto.cs
- [x] T222 [US7] Create GetUtilizationReportQuery in Maliev.LeaveService.Application/Queries/GetUtilizationReportQuery.cs with optional departmentId filter
- [x] T223 [US7] Create GetUtilizationReportQueryHandler in Maliev.LeaveService.Application/Queries/Handlers/GetUtilizationReportQueryHandler.cs with aggregation logic:
  - Total leave days taken by type
  - Average balance utilization
  - Peak leave periods identification
  - Department filtering if specified
- [x] T224 [US7] Create LeaveReportsController in Maliev.LeaveService.Api/Controllers/LeaveReportsController.cs with GET /leave/v1/reports/utilization endpoint
- [x] T225 [US7] Add optional departmentId query parameter to utilization endpoint
- [x] T226 [US7] Add permission authorization using [RequirePermission(LeavePermissions.Reports)]
- [x] T227 [US7] Register GetUtilizationReportQueryHandler in Program.cs
- [ ] T228 [US7] Add logging for report generation operations

**Checkpoint**: All user stories now complete. Full leave management system operational from submission to reporting.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and operational readiness

- [ ] T229 [P] Create LeaveIAMRegistrationService in Maliev.LeaveService.Infrastructure/Services/LeaveIAMRegistrationService.cs to register permissions with IAM service
- [ ] T230 [P] Register LeaveIAMRegistrationService in Program.cs using builder.Services.AddHostedService
- [ ] T231 [P] Add business metrics instrumentation to all handlers (leave requests submitted, average approval time, utilization rate, accrual duration)
- [ ] T232 [P] Review and optimize database queries for performance (ensure AsNoTracking, projections, proper indexes)
- [ ] T233 [P] Review and validate all permission checks across controllers
- [ ] T234 [P] Review and validate all audit logging coverage (FR-027 7-year retention)
- [ ] T235 [P] Add integration tests for event schema validation in Maliev.LeaveService.Tests/Contract/Events/EventSchemaContractTests.cs
- [ ] T236 [P] Verify all error responses follow standard format with appropriate HTTP status codes
- [ ] T237 [P] Add repository-level unit tests for complex queries (overlap detection, balance calculation, accrual logic)
- [ ] T238 [P] Review connection pooling configuration (max 20 connections per plan.md)
- [ ] T239 [P] Review Redis cache invalidation strategy
- [ ] T240 [P] Update README.md with:
  - Architecture diagram
  - Local development setup with Testcontainers
  - Environment variables documentation
  - API endpoint reference
  - Data migration guide from Employee Service
- [ ] T241 [P] Create API documentation examples for Scalar UI
- [ ] T242 [P] Verify Dockerfile follows all constitution requirements (BuildKit secrets, health check, app user, port 8080)
- [ ] T243 [P] Verify CI/CD workflows follow constitution naming (ci-develop.yml, ci-staging.yml, ci-main.yml)
- [ ] T244 [P] Run full test suite with real Testcontainers (PostgreSQL, RabbitMQ, Redis) and verify 80% coverage
- [ ] T245 [P] Security review: validate JWT authentication, permission authorization, secrets management
- [ ] T246 [P] Performance testing: verify p95 < 500ms for queries, < 1s for commands under load
- [ ] T247 [P] Load testing: verify system handles 50-200 requests/day within resource constraints (128-192MB memory, 10-40m CPU)
- [ ] T248 Code cleanup and refactoring for consistency
- [ ] T249 Final build verification with zero warnings (TreatWarningsAsErrors=true)
- [ ] T250 Prepare data migration scripts for production cutover from Employee Service

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-9)**: All depend on Foundational phase completion
  - User Story 1 (P1): Can start after Foundational
  - User Story 2 (P1): Can start after Foundational (integrates with US1 but independently testable)
  - User Story 3 (P2): Can start after Foundational (integrates with US1 but independently testable)
  - User Story 4 (P2): Can start after Foundational (integrates with US1/US2 but independently testable)
  - User Story 5 (P3): Can start after Foundational (independent - policy admin)
  - User Story 6 (P3): Can start after Foundational (independent - background accrual)
  - User Story 7 (P3): Can start after Foundational (independent - reporting)
- **Polish (Phase 10)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies on other stories - submit leave requests
- **User Story 2 (P1)**: Integrates with US1 (approves requests from US1) but independently testable
- **User Story 3 (P2)**: Integrates with US1 (views balances/history from US1) but independently testable
- **User Story 4 (P2)**: Integrates with US1/US2 (cancels requests from US1/US2) but independently testable
- **User Story 5 (P3)**: Independent - HR policy administration
- **User Story 6 (P3)**: Independent - automated accrual background process
- **User Story 7 (P3)**: Independent - reporting and analytics

### Within Each User Story

Per constitution Test-First Development requirement:

1. **Tests FIRST**: All test tasks must be completed and verified as FAILING before implementation
2. **Models**: Domain entities before services
3. **Services**: Application handlers before controllers
4. **Endpoints**: Controllers after handlers are implemented
5. **Integration**: Events and notifications after core functionality
6. **Story Complete**: Verify independent testability before moving to next story

### Parallel Opportunities

- **All Setup tasks marked [P]** in Phase 1 can run in parallel (T002-T006, T008-T021)
- **All Foundational tasks marked [P]** in Phase 2 can run in parallel within categories (enums T023-T025, entities T028-T030, configurations T033-T036, Program.cs additions T042-T049, Test utilities T058-T060, interfaces T062-T065)
- **Once Foundational completes**: All user stories can start in parallel if team capacity allows
- **Within each user story**: All test tasks marked [P] can run in parallel
- **Within each user story**: All model/DTO tasks marked [P] can run in parallel
- **Different user stories**: Can be worked on in parallel by different team members after Foundational phase

---

## Parallel Example: User Story 1

```bash
# After Foundational Phase completes, launch all tests for User Story 1 together:
# (All must FAIL before implementation begins)
Task: "Contract test for GET /leave/v1/balances/{employeeId} endpoint in Maliev.LeaveService.Tests/Contract/Api/LeaveBalancesApiContractTests.cs"
Task: "Contract test for POST /leave/v1/requests/{employeeId} endpoint in Maliev.LeaveService.Tests/Contract/Api/LeaveRequestsApiContractTests.cs"
Task: "Integration test for leave request submission with sufficient balance in Maliev.LeaveService.Tests/Integration/LeaveRequestSubmissionTests.cs"
Task: "Integration test for leave request rejection with insufficient balance in Maliev.LeaveService.Tests/Integration/LeaveRequestSubmissionTests.cs"
Task: "Integration test for overlapping request detection in Maliev.LeaveService.Tests/Integration/LeaveRequestSubmissionTests.cs"
Task: "Integration test for half-day leave request (morning/afternoon) in Maliev.LeaveService.Tests/Integration/LeaveRequestSubmissionTests.cs"
Task: "Integration test for date validation (start date after end date, duration > 30 days) in Maliev.LeaveService.Tests/Integration/LeaveRequestSubmissionTests.cs"
Task: "Integration test for 24-hour advance notice validation in Maliev.LeaveService.Tests/Integration/LeaveRequestSubmissionTests.cs"
Task: "Integration test for Employee Service unavailability (queue for delayed validation) in Maliev.LeaveService.Tests/Integration/LeaveRequestSubmissionTests.cs"
Task: "Unit test for balance calculation logic in Maliev.LeaveService.Tests/Unit/Domain/LeaveBalanceTests.cs"
Task: "Unit test for overlap detection logic in Maliev.LeaveService.Tests/Unit/Validation/OverlapDetectionTests.cs"
Task: "Unit test for SubmitLeaveRequestCommandHandler in Maliev.LeaveService.Tests/Unit/Handlers/SubmitLeaveRequestCommandHandlerTests.cs"

# After ALL tests are written and FAILING, launch all DTOs together:
Task: "Create SubmitLeaveRequestDto in Maliev.LeaveService.Application/DTOs/Requests/SubmitLeaveRequestDto.cs with Data Annotations validation"
Task: "Create LeaveBalanceDto in Maliev.LeaveService.Application/DTOs/Responses/LeaveBalanceDto.cs"
Task: "Create LeaveRequestDto in Maliev.LeaveService.Application/DTOs/Responses/LeaveRequestDto.cs"

# Then launch repositories in parallel:
Task: "Create LeaveRequestRepository in Maliev.LeaveService.Infrastructure/Repositories/LeaveRequestRepository.cs implementing ILeaveRequestRepository"
Task: "Create LeaveBalanceRepository in Maliev.LeaveService.Infrastructure/Repositories/LeaveBalanceRepository.cs implementing ILeaveBalanceRepository"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only) - Recommended Approach

1. Complete **Phase 1: Setup** (T001-T021) - Project structure and dependencies
2. Complete **Phase 2: Foundational** (T022-T065) - CRITICAL blocking infrastructure
3. Complete **Phase 3: User Story 1** (T066-T098) - Employee submits leave request with full validation
4. **STOP and VALIDATE**: Run all User Story 1 tests independently, verify end-to-end functionality
5. Deploy/demo MVP if ready

**This delivers the core value proposition**: Employees can digitally submit leave requests with balance validation, date validation, overlap detection, and advance notice requirements.

### Incremental Delivery (Add Stories Sequentially)

1. Complete **Setup + Foundational** → Foundation ready
2. Add **User Story 1 (P1)** → Test independently → Deploy/Demo (MVP!)
3. Add **User Story 2 (P1)** → Test independently → Deploy/Demo (Complete approval workflow)
4. Add **User Story 3 (P2)** → Test independently → Deploy/Demo (Balance visibility and history)
5. Add **User Story 4 (P2)** → Test independently → Deploy/Demo (Cancellation capability)
6. Add **User Story 5 (P3)** → Test independently → Deploy/Demo (Policy administration)
7. Add **User Story 6 (P3)** → Test independently → Deploy/Demo (Automated accrual)
8. Add **User Story 7 (P3)** → Test independently → Deploy/Demo (Reporting)
9. Complete **Polish (Phase 10)** → Production-ready

Each story adds value without breaking previous stories. This approach enables continuous delivery and early user feedback.

### Parallel Team Strategy (If Multiple Developers Available)

With 2-3 developers after Foundational phase completion:

1. **Team completes Setup + Foundational together** (T001-T065)
2. **Once Foundational is done**, assign parallel work:
   - **Developer A**: User Story 1 (P1) - T066-T098
   - **Developer B**: User Story 2 (P1) - T099-T128
   - **Developer C**: User Story 5 (P3) - T163-T192 (independent policy admin)
3. **After P1 stories complete**, continue with:
   - **Developer A**: User Story 3 (P2) - T129-T147
   - **Developer B**: User Story 4 (P2) - T148-T162
   - **Developer C**: User Story 6 (P3) - T193-T214
4. **Final sprint**: User Story 7 + Polish

Stories complete and integrate independently, enabling parallel development with minimal merge conflicts.

---

## Summary

- **Total Tasks**: 250
- **Setup Phase**: 21 tasks (T001-T021)
- **Foundational Phase**: 44 tasks (T022-T065) - BLOCKS all user stories
- **User Story 1 (P1)**: 33 tasks (T066-T098) - MVP 🎯
- **User Story 2 (P1)**: 30 tasks (T099-T128)
- **User Story 3 (P2)**: 19 tasks (T129-T147)
- **User Story 4 (P2)**: 15 tasks (T148-T162)
- **User Story 5 (P3)**: 30 tasks (T163-T192)
- **User Story 6 (P3)**: 22 tasks (T193-T214)
- **User Story 7 (P3)**: 14 tasks (T215-T228)
- **Polish Phase**: 22 tasks (T229-T250)

**Parallel Opportunities**:
- Setup: 16 tasks can run in parallel
- Foundational: 28 tasks can run in parallel within phase
- User Stories: All 7 stories can start in parallel after Foundational
- Within each story: Tests, models, and DTOs can run in parallel

**Independent Test Criteria** verified for each story per specification.

**MVP Recommendation**: Complete through User Story 1 (Phase 1 + 2 + 3 = 98 tasks total) for fastest time-to-value.

**Test-First Compliance**: All test tasks marked with constitution requirement to write tests FIRST and verify they FAIL before implementation begins.
