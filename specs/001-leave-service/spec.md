# Feature Specification: Employee Leave Management Service

**Feature Branch**: `001-leave-service`
**Created**: 2025-12-28
**Status**: Draft
**Input**: User description: "Maliev.LeaveService is a new microservice responsible for managing employee leave requests, balances, approvals, and policies. This service will be extracted from the existing Employee Service."

## Clarifications

### Session 2025-12-28

- Q: What is the minimum advance notice required for non-sick leave requests? → A: 24 hours minimum advance notice for all leave types except sick leave
- Q: How should half-day leave be defined and tracked? → A: First half (morning) or second half (afternoon) of workday
- Q: What should happen when the Employee Service is unavailable during leave request submission? → A: Allow submission with delayed validation (queue for processing when service recovers)
- Q: How many days before expiration should the system alert employees about expiring carried forward leave? → A: 30 days before expiration
- Q: How long must audit logs for leave transactions be retained? → A: 7 years (employment records legal compliance)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Employee Submits Leave Request (Priority: P1)

An employee wants to request time off from work. They check their available leave balance, select the dates they want to take off, specify the type of leave, and submit the request for manager approval.

**Why this priority**: This is the core value proposition of the service. Without the ability to submit leave requests, the service provides no value to employees.

**Independent Test**: Can be fully tested by creating an employee account, logging in, viewing leave balances, submitting a leave request, and verifying the request appears in pending status. Delivers immediate value by digitizing the leave request process.

**Acceptance Scenarios**:

1. **Given** an employee has 15 days of annual leave available, **When** they submit a request for 5 days of annual leave with valid dates at least 24 hours in advance, **Then** the system creates a pending leave request and reduces available balance by 5 days
2. **Given** an employee has 2 days of sick leave available, **When** they attempt to submit a request for 5 days of sick leave, **Then** the system rejects the request with an insufficient balance error
3. **Given** an employee has an approved leave request from Jan 10-15, **When** they attempt to submit another request for Jan 12-14, **Then** the system rejects the request with an overlapping dates error
4. **Given** an employee wants to take a half-day off, **When** they submit a half-day leave request for the morning of Jan 20, **Then** the system deducts 0.5 days from their balance
5. **Given** an employee submits a leave request, **When** the start date is after the end date or the duration exceeds 30 consecutive days, **Then** the system rejects the request with a validation error
6. **Given** an employee attempts to submit an annual leave request for tomorrow (less than 24 hours notice), **When** they submit the request, **Then** the system rejects it with an advance notice requirement error
7. **Given** the Employee Service is temporarily unavailable, **When** an employee submits a leave request, **Then** the system accepts the submission and queues it for validation once the service recovers, notifying the employee of the delayed processing

---

### User Story 2 - Manager Approves or Rejects Leave Requests (Priority: P1)

A manager needs to review pending leave requests from their team members. They view the request details including dates, reason, and current team coverage, then approve or reject the request with optional comments.

**Why this priority**: The approval workflow is essential for the leave management process. Without approvals, requests cannot be finalized and employees don't know if they can take time off.

**Independent Test**: Can be fully tested by having a manager log in, view their pending approvals, and approve or reject a request with comments. Delivers value by enabling managers to control team availability.

**Acceptance Scenarios**:

1. **Given** a manager has pending leave requests from their team, **When** they view their approval queue, **Then** they see all pending requests with employee name, dates, leave type, and reason
2. **Given** a manager is reviewing a leave request, **When** they approve it with comments, **Then** the request status changes to approved, the employee is notified, and the leave days are deducted from the employee's balance
3. **Given** a manager is reviewing a leave request, **When** they reject it with a reason, **Then** the request status changes to rejected, the employee is notified, and the pending days are returned to the employee's available balance
4. **Given** a leave request requires multi-level approval, **When** the first manager approves it, **Then** the request moves to the next approval level
5. **Given** an HR administrator views any pending request, **When** they approve or reject it, **Then** the decision is final regardless of the normal approval hierarchy

---

### User Story 3 - Employee Views Leave Balance and History (Priority: P2)

An employee wants to see how many leave days they have available for each leave type, including carried forward days and expiration dates. They also want to view their leave request history to see past and upcoming time off.

**Why this priority**: Transparency in leave balances helps employees plan their time off effectively. This is important but not as critical as the ability to request and approve leave.

**Independent Test**: Can be fully tested by logging in as an employee and viewing their leave balance dashboard showing entitled, used, pending, and available days for each leave type. Delivers value by enabling self-service leave planning.

**Acceptance Scenarios**:

1. **Given** an employee has 20 entitled annual leave days, 5 used, and 3 pending, **When** they view their leave balance, **Then** they see 12 available days
2. **Given** an employee carried forward 5 annual leave days from the previous year with expiration date March 31, **When** they view their balance in February, **Then** they see the carried forward days and expiration warning
3. **Given** an employee wants to plan future leave, **When** they view their leave history, **Then** they see all approved, pending, and rejected requests with dates and status
4. **Given** an employee views their balance for a specific year, **When** they select a different year, **Then** the system shows historical balance data for that year
5. **Given** an employee has carried forward leave days expiring in 30 days, **When** the system runs its daily alert process, **Then** the employee receives a notification warning them about the upcoming expiration

---

### User Story 4 - Employee Cancels Pending or Approved Leave Request (Priority: P2)

An employee needs to cancel a previously submitted leave request due to changed plans. They select the request and cancel it with an optional reason.

**Why this priority**: Life changes happen and employees need flexibility to cancel plans. This is important for user experience but not essential for MVP.

**Independent Test**: Can be fully tested by submitting a leave request, then canceling it, and verifying the days are returned to available balance.

**Acceptance Scenarios**:

1. **Given** an employee has a pending leave request, **When** they cancel it with a reason, **Then** the request status changes to cancelled and the pending days are returned to their available balance
2. **Given** an employee has an approved leave request that hasn't started yet, **When** they cancel it, **Then** the request status changes to cancelled, the used days are returned to their available balance, and the manager is notified
3. **Given** an employee has an approved leave request that has already started, **When** they attempt to cancel it, **Then** the system shows a warning requiring manager confirmation

---

### User Story 5 - HR Administers Leave Policies (Priority: P3)

An HR administrator needs to configure leave policies for the organization, defining entitlements, accrual rates, carry-forward rules, and approval requirements for each leave type.

**Why this priority**: Policy configuration is needed for system setup and annual updates, but most organizations have stable policies that don't change frequently.

**Independent Test**: Can be fully tested by logging in as HR admin, creating or updating a leave policy, and verifying the policy applies to employee entitlements.

**Acceptance Scenarios**:

1. **Given** an HR admin creates a new annual leave policy, **When** they set 20 days default entitlement with monthly accrual of 1.67 days, **Then** new employees receive this entitlement
2. **Given** an HR admin updates a leave policy, **When** they set maximum carry-forward to 5 days with 3-month expiration, **Then** employees can only carry forward up to 5 days and they expire after 3 months into the new year
3. **Given** an HR admin configures a leave type, **When** they set it to require 2-level approval, **Then** all requests for that leave type must be approved by both direct manager and department head
4. **Given** an HR admin deactivates a leave policy, **When** employees try to request that leave type, **Then** the system prevents new requests but preserves historical data

---

### User Story 6 - System Automatically Accrues Leave Balances (Priority: P3)

The system automatically accrues leave balances for all employees on a monthly basis according to their leave policies, handling pro-ration for new hires and terminations.

**Why this priority**: Automated accrual reduces manual HR work, but it's a background process that can be handled manually in early stages if needed.

**Independent Test**: Can be fully tested by configuring an accrual policy, simulating month-end processing, and verifying employee balances are updated correctly.

**Acceptance Scenarios**:

1. **Given** it is the 1st day of a new month, **When** the monthly accrual process runs, **Then** all active employees receive their monthly accrued leave days based on their policy
2. **Given** an employee started mid-month on the 15th, **When** the monthly accrual runs, **Then** they receive pro-rated leave days for the partial month worked
3. **Given** an employee terminated on the 20th of the month, **When** the monthly accrual runs, **Then** they receive pro-rated leave days and no further accruals
4. **Given** an employee has carried forward days from the previous year, **When** the expiration date passes, **Then** the expired carried forward days are automatically removed from their balance

---

### User Story 7 - HR and Managers View Leave Utilization Reports (Priority: P3)

HR administrators and managers need visibility into leave utilization patterns across the organization or department to plan for coverage and identify trends.

**Why this priority**: Reporting provides strategic value but is not essential for day-to-day leave management operations.

**Independent Test**: Can be fully tested by generating a utilization report for a department and verifying it shows leave usage statistics and trends.

**Acceptance Scenarios**:

1. **Given** an HR admin wants to see organization-wide leave utilization, **When** they generate a report, **Then** they see total leave days taken by type, average balance utilization, and peak leave periods
2. **Given** a manager wants to see their team's leave patterns, **When** they filter the report by their department, **Then** they see only their team members' leave data
3. **Given** an HR admin analyzes leave trends, **When** they view historical utilization data, **Then** they can identify seasonal patterns and plan staffing accordingly

---

### Edge Cases

- What happens when an employee submits a same-day leave request (e.g., calling in sick today)? Answer: Only sick leave can be submitted same-day (FR-022); all other leave types require 24 hours advance notice.
- How does the system handle leave requests that span across year boundaries (e.g., Dec 28 - Jan 3)? Answer: System splits the days correctly across year boundaries (FR-029).
- What happens when an employee is terminated while having approved future leave requests? Answer: System settles balances via integration with Employee Service termination event (FR-025).
- How does the system handle manual balance adjustments by HR (e.g., correcting errors or granting additional leave)? Answer: HR can manually adjust balances with full audit logging (FR-020).
- What happens when a manager who has pending approvals is reassigned or leaves the organization? Answer: System reassigns pending approvals automatically (FR-030).
- How does the system handle half-day leave requests when calculating available balance? Answer: Half-day requests (morning or afternoon) deduct 0.5 days from balance; two different half-days on same day are allowed.
- What happens when leave policies change mid-year and employees have pending or approved requests under old policies? Answer: Existing requests honor the policy in effect at time of submission.
- How does the system prevent employees from gaming the system by submitting multiple overlapping requests? Answer: System validates for overlaps before accepting any request (FR-003).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow employees to view their current leave balances for all leave types showing entitled, used, pending, and available days
- **FR-002**: System MUST allow employees to submit leave requests specifying leave type, start date, end date, and optional reason
- **FR-003**: System MUST validate leave requests to ensure start date is not after end date, duration does not exceed 30 consecutive days, start date is at least 24 hours in the future for non-sick leave types, and no overlapping with existing approved or pending requests
- **FR-004**: System MUST check that employees have sufficient available balance before accepting a leave request
- **FR-005**: System MUST support full-day and half-day leave requests (morning or afternoon) with accurate balance calculations, where half-day equals 0.5 days
- **FR-006**: System MUST automatically update employee's pending balance when a request is submitted and adjust used/available balance when approved
- **FR-007**: System MUST allow managers to view all pending leave requests from their direct reports
- **FR-008**: System MUST allow managers to approve or reject leave requests with optional comments
- **FR-009**: System MUST support multi-level approval workflows where requests can require approval from multiple managers in sequence
- **FR-010**: System MUST allow HR administrators to approve or reject any pending leave request, overriding normal approval hierarchy
- **FR-011**: System MUST send notifications to employees when their leave request is approved, rejected, or requires action
- **FR-012**: System MUST allow employees to cancel pending leave requests, immediately returning days to their available balance
- **FR-013**: System MUST allow employees to cancel approved leave requests with manager notification
- **FR-014**: System MUST support multiple leave types including annual, sick, personal, maternity, paternity, unpaid, bereavement, and study leave
- **FR-015**: System MUST allow HR administrators to create and configure leave policies defining entitlement, accrual rate, carry-forward rules, and approval requirements
- **FR-016**: System MUST automatically accrue leave balances monthly based on each employee's leave policy
- **FR-017**: System MUST support pro-rated leave accrual for employees who join or leave mid-month
- **FR-018**: System MUST support carry-forward of unused leave days from one year to the next with configurable maximum and expiration rules
- **FR-019**: System MUST automatically expire carried forward leave days after the configured expiration period
- **FR-020**: System MUST allow HR administrators to manually adjust leave balances with audit logging of all adjustments
- **FR-021**: System MUST prevent employees from requesting leave that would result in negative balance
- **FR-022**: System MUST allow same-day leave requests for sick leave only
- **FR-023**: System MUST allow employees to view their complete leave history including pending, approved, rejected, and cancelled requests
- **FR-024**: System MUST generate leave utilization reports showing usage patterns by department, leave type, and time period
- **FR-025**: System MUST integrate with employee service to initialize leave balances for new employees and settle balances for terminated employees
- **FR-031**: System MUST gracefully handle Employee Service unavailability by queueing leave requests for delayed validation and processing when the service recovers
- **FR-026**: System MUST enforce permission-based access control ensuring employees can only view and manage their own leave, managers can approve their team's requests, and HR has full administrative access
- **FR-027**: System MUST maintain audit logs of all leave requests, approvals, rejections, cancellations, and balance adjustments, retaining records for a minimum of 7 years to meet employment records legal compliance requirements
- **FR-028**: System MUST send alerts to employees 30 days before their carried forward leave days expire
- **FR-029**: System MUST handle requests that span multiple years by splitting the days correctly across year boundaries
- **FR-030**: System MUST reassign pending approvals when a manager is reassigned or terminated

### Key Entities

- **Leave Request**: Represents an employee's request for time off, including the leave type, date range, total days requested, half-day indicator (full-day, morning, or afternoon), reason, and current approval status with audit trail of all approval decisions
- **Leave Balance**: Tracks an employee's leave entitlement and usage for each leave type in a specific year, including entitled days, used days, pending days, carried forward days from previous year, and available days
- **Leave Approval**: Represents a single approval decision in the workflow, including the approver, approval level, decision (approved/rejected), comments, and decision timestamp
- **Leave Policy**: Defines the rules for a specific leave type including default entitlement, monthly accrual rate, maximum carry-forward allowed, expiration period for carried forward days, and approval workflow requirements
- **Leave Type**: Categorizes different types of leave such as annual, sick, personal, maternity, paternity, unpaid, bereavement, and study leave, each potentially having different policies and rules

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Employees can submit a leave request in under 2 minutes from login to submission confirmation
- **SC-002**: Managers can review and approve or reject a leave request in under 1 minute
- **SC-003**: Employees can view their current leave balance and request history within 5 seconds of navigation
- **SC-004**: System accurately calculates and displays available leave balance reflecting all pending and approved requests
- **SC-005**: Leave balance accrual completes for all employees within 1 hour on the 1st of each month
- **SC-006**: 95% of leave requests follow the standard single-level approval workflow without requiring HR intervention
- **SC-007**: Employees receive approval or rejection notification within 1 minute of manager decision
- **SC-008**: System maintains 100% accuracy in balance calculations with zero instances of negative balances or double-booking
- **SC-009**: HR administrators can configure or modify a leave policy in under 5 minutes
- **SC-010**: Leave utilization reports generate within 10 seconds for organization-wide queries covering up to 1 year of data
- **SC-011**: System successfully integrates with employee service to auto-initialize leave balances for new hires within 24 hours of onboarding
- **SC-012**: Audit logs capture 100% of all leave transactions for compliance and troubleshooting purposes, with 7-year retention for legal compliance
- **SC-013**: Employees rate the leave request process as "easy" or "very easy" in 90% of user satisfaction surveys
- **SC-014**: System reduces HR administrative time spent on manual leave tracking by 80% compared to spreadsheet-based process

## Assumptions

- The service will integrate with an existing Employee Service that provides employee master data including employee ID, department, manager assignment, and employment status
- Authentication and user identity management is handled by a separate authentication service
- The organization uses a standard Monday-Friday work week for leave calculations
- Public holidays and weekends are managed separately and do not count against leave balances
- The system assumes a standard full-time employee works 5 days per week (half-day = 0.5 days)
- Email or push notification infrastructure exists to deliver notifications to employees and managers
- Leave policies are defined at the organization level, not customized per employee (though HR can make manual adjustments)
- The system will initially support a single organization/tenant with potential for multi-tenancy in future
- Business days and working hours are defined consistently across the organization
- Manager hierarchy is maintained in the Employee Service and accessible via integration events
- Carried forward leave days from legacy systems can be manually entered by HR during initial setup

## Dependencies

- **Employee Service**: Provides employee master data, organizational structure, and manager assignments. Must publish events when employees are created, terminated, or reassigned. Failure mode: Leave requests are queued for delayed validation when service is unavailable
- **Authentication Service**: Provides user authentication and authorization tokens with employee identity and role information
- **Notification Service**: Delivers email and/or push notifications to employees and managers for leave request status updates and expiration alerts
- **Audit Logging Infrastructure**: Centralized audit logging system to record all leave transactions for compliance and security

## Constraints

- Leave request duration cannot exceed 30 consecutive days
- Employees cannot submit overlapping leave requests (except for half-day requests on the same day if one is morning and one is afternoon)
- Only sick leave requests can be submitted for same-day or past dates
- Carried forward leave days must expire within the timeframe defined by the leave policy (typically 3-12 months into the new year)
- Multi-level approval workflows cannot exceed 5 approval levels
- Leave balance accrual runs automatically on the 1st of each month and cannot be manually triggered mid-month
- Historical leave data cannot be deleted, only marked as cancelled or adjusted with audit trail
- Audit logs must be retained for a minimum of 7 years for legal compliance

## Out of Scope

The following items are explicitly out of scope for this feature:

- Public holiday management and calendar integration
- Integration with payroll systems for leave payout calculations
- Time-off-in-lieu (TOIL) or compensatory time off tracking
- Shift worker leave management with non-standard work schedules
- Mobile application development (service will provide APIs only)
- Employee self-service portal UI (service provides backend APIs, UI is separate)
- Integration with external calendar systems (Google Calendar, Outlook)
- Leave request delegation (e.g., assistant submitting leave on behalf of executive)
- Approval delegation (e.g., manager delegating approval authority while on vacation)
- Advanced analytics and predictive leave planning
- Multi-currency leave policies or location-based leave rules for international employees
- Integration with resource planning or project management tools
- Workflow automation rules (e.g., auto-approve requests under certain conditions)
