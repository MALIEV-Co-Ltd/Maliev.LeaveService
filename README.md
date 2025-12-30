# Leave Service

Dedicated microservice for managing employee leave requests, balances, and policies for Maliev Co. Ltd.

## Overview

The Leave Service manages all aspects of employee time off, including:

- **Leave Requests** - Submission, approval, rejection, and cancellation of time-off requests.
- **Leave Balances** - Real-time tracking of entitled, used, pending, and remaining leave days.
- **Leave Policies** - Configuration of entitlement rules, carry-over limits, and approval requirements for different leave types.
- **Accrual Processing** - Automated monthly accrual of leave days based on company policy.

## Architecture

- **Framework**: ASP.NET Core 10.0
- **Database**: PostgreSQL 18 with Entity Framework Core
- **Messaging**: RabbitMQ via MassTransit (consumes employee lifecycle events)
- **API Documentation**: OpenAPI with Scalar UI

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- PostgreSQL 18
- Docker (optional, for Redis and RabbitMQ)

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/MALIEV-Co-Ltd/Maliev.LeaveService.git
   ```

2. **Run database migrations**
   ```bash
   dotnet ef database update --project Maliev.LeaveService.Infrastructure --startup-project Maliev.LeaveService.Api
   ```

3. **Run the service**
   ```bash
   dotnet run --project Maliev.LeaveService.Api
   ```

   The service will be available at `https://localhost:7055` or `http://localhost:5276`.

## API Endpoints

### Leave Requests

```
POST /leave/v1/LeaveRequests/{employeeId} - Submit a new leave request
GET  /leave/v1/LeaveRequests/employee/{employeeId} - Get all requests for an employee
GET  /leave/v1/LeaveRequests/pending/{managerId} - Get pending approvals for a manager
POST /leave/v1/LeaveRequests/{requestId}/approve - Approve a request
POST /leave/v1/LeaveRequests/{requestId}/reject - Reject a request
PUT  /leave/v1/LeaveRequests/{requestId}/cancel - Cancel a request
```

### Leave Balances

```
GET /leave/v1/LeaveBalances/{employeeId} - Get current leave balances
```

### Leave Types & Reports

```
GET /leave/v1/LeaveTypes - List all available leave types and policies
GET /leave/v1/Reports/utilization - Get organizational leave utilization report
```

## Integration Events Consumed

- `EmployeeCreatedIntegrationEvent` - Initializes leave balances for new hires.
- `EmployeeTerminatedIntegrationEvent` - Cancels pending leave requests for departing employees.

## License

Copyright © 2025 Maliev Co. Ltd. All rights reserved.
