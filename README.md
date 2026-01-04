# Maliev Leave Service

[![Build Status](https://img.shields.io/badge/Build-Passing-success)](https://github.com/ORGANIZATION/Maliev.LeaveService)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Database](https://img.shields.io/badge/Database-PostgreSQL%2018-blue)](https://www.postgresql.org/)

Dedicated microservice for managing employee leave requests, balances, and time-off policies.

**Role in MALIEV Architecture**: The authoritative system for managing employee availability. It handles the complete lifecycle of leave requests and balance tracking, integrating with Employee and Notification services to manage approvals and publishing events that influence payroll and project scheduling.

---

## 🏗️ Architecture & Tech Stack

- **Framework**: ASP.NET Core 10.0 (C# 13)
- **Database**: PostgreSQL 18 with Entity Framework Core 10.x
- **Distributed Cache**: Redis 7.x (High-frequency balance resolution)
- **Messaging**: RabbitMQ via MassTransit
- **API Documentation**: OpenAPI 3.1 + Scalar UI
- **Observability**: OpenTelemetry (Metrics, Traces, Logging)

---

## ⚖️ Constitution Rules

This service strictly adheres to the platform development mandates:

### Banned Libraries
To maintain high performance and low complexity, the following are **NOT** used:
- ❌ **AutoMapper**: Explicit manual mapping only.
- ❌ **FluentValidation**: Standard Data Annotations (`[Required]`, `[EmailAddress]`) only.
- ❌ **FluentAssertions**: Standard xUnit `Assert` methods only.
- ❌ **In-memory Test DB**: All integration tests use **Testcontainers** with real PostgreSQL 18.

### Mandatory Practices
- ✅ **TreatWarningsAsErrors**: Enabled in all `.csproj` files.
- ✅ **XML Documentation**: Required on all public methods and properties.
- ✅ **No Secrets in Code**: All sensitive configuration injected via environment variables.
- ✅ **No Test Config in Program.cs**: Test configuration in test fixtures only.
- ✅ **IAM Integration**: Self-registers permissions with the IAM Service using GCP-style naming: `{service}.{resource}.{action}`.

---

## ✨ Key Features

- **Dynamic Leave Requests**: Streamlined submission and tracking of various time-off types with automated approval routing.
- **Precision Balance Tracking**: Real-time calculation of entitled, used, pending, and remaining leave days.
- **Rule-Based Policies**: Configurable entitlement rules, carry-over limits, and accrual frequencies per department or role.
- **Automated Accrual Engine**: Batch processing for monthly and annual leave day increments based on tenure.
- **Organizational Visibility**: Specialized reporting for managers and HR to track team availability and utilization.

---

## 🚀 Quick Start

### Prerequisites
- .NET 10.0 SDK
- Docker Desktop (for infrastructure)
- PostgreSQL 18 (Alpine)

### Local Development Setup

1. **Clone the repository**
```bash
git clone https://github.com/ORGANIZATION/Maliev.LeaveService.git
cd Maliev.LeaveService
```

2. **Spin up Infrastructure**
```bash
docker run --name leave-db -e POSTGRES_PASSWORD=YOUR_PASSWORD -p 5432:5432 -d postgres:18-alpine
docker run --name leave-redis -p 6379:6379 -d redis:7-alpine
```

3. **Configure Environment**
```powershell
# Windows PowerShell
$env:ConnectionStrings__LeaveDbContext="YOUR_POSTGRES_CONNECTION_STRING"
$env:ConnectionStrings__Cache="YOUR_REDIS_CONNECTION_STRING"
```

4. **Apply Migrations & Run**
```bash
dotnet ef database update --project Maliev.LeaveService.Infrastructure --startup-project Maliev.LeaveService.Api
dotnet run --project Maliev.LeaveService.Api
```

The service will be available at `http://localhost:5000/leave`. Access the interactive documentation at `http://localhost:5000/leave/scalar`.

---

## 📡 API Endpoints

All endpoints are prefixed with `/leave/v1/`.

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/LeaveRequests` | Submit a new leave request |
| GET | `/LeaveBalances/{employeeId}` | Get current leave balances |
| POST | `/LeaveRequests/{id}/approve` | Approve a pending request |
| GET | `/LeaveTypes` | List active policies and leave types |

---

## 🏥 Health & Monitoring

Standardized health probes for Kubernetes orchestration:
- **Liveness**: `GET /leave/liveness`
- **Readiness**: `GET /leave/readiness` (Checks DB and Redis connectivity)
- **Metrics**: `GET /leave/metrics` (Prometheus format)

---

## 🧪 Testing

We prioritize reliable tests over mock-heavy unit tests.

```bash
# Run all tests using Testcontainers
dotnet test --verbosity normal
```

- **Integration Tests**: Use real PostgreSQL 18 containers.
- **Contract Tests**: Ensure API stability for consumers.

---

## 📦 Deployment

Infrastructure management is handled via GitOps patterns.

- **Docker Image**: `REGION-docker.pkg.dev/PROJECT_ID/REPOSITORY/maliev-leave-service:{sha}`
- **Environments**: Development, Staging, Production

---

## 📄 License

Proprietary - © 2025 MALIEV Co., Ltd. All rights reserved.
