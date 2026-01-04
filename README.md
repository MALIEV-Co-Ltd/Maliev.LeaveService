# Maliev Leave Service

[![Build Status](https://img.shields.io/badge/Build-Passing-success)](https://github.com/ORGANIZATION/Maliev.LeaveService)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Database](https://img.shields.io/badge/Database-PostgreSQL%2018-blue)](https://www.postgresql.org/)

Microservice for managing employee leave requests, balances, and corporate policies.

**Role in MALIEV Architecture**: Handles the lifecycle of time-off requests. It tracks balances in real-time and integrates with the Employee Service for identity validation and Notification Service for manager alerts.

---

## 🏗️ Architecture & Tech Stack

- **Framework**: ASP.NET Core 10.0
- **Database**: PostgreSQL 18
- **Cache**: Redis 7.x
- **Messaging**: RabbitMQ (Consumes Employee events)
- **API Documentation**: OpenAPI 3.1 + Scalar UI

---

## ⚖️ Constitution Rules

### Banned Libraries
- ❌ **AutoMapper**
- ❌ **FluentValidation**
- ❌ **FluentAss
