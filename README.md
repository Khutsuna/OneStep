# OneStep

OneStep is a full-stack e-commerce platform built as an IT Step Academy diploma project. Customers browse a product catalogue, search and filter it, and order products; store managers and administrators run the catalogue, stock, orders, campaigns and users from an admin panel.

> **Status:** in development. Planned completion: November 2026.

## Tech stack

| Layer | Technology |
| --- | --- |
| Frontend | Angular 22, TypeScript, Angular Material, NgRx Signal Store, SCSS |
| Backend | ASP.NET Core 10 Web API (C#), clean architecture |
| Data access | Entity Framework Core 10 (code-first, migrations) |
| Database | Microsoft SQL Server 2025, with hand-written SQL scripts |
| Auth | JWT bearer tokens, role-based authorization (Admin, Store Manager, Customer) |
| Testing | xUnit |

## Repository structure

```
onestep/
├─ backend/
│  ├─ OneStep.slnx
│  ├─ src/
│  │  ├─ OneStep.Api              controllers, middleware, startup
│  │  ├─ OneStep.Application      services, DTOs, validators, interfaces
│  │  ├─ OneStep.Domain           entities, enums, business rules
│  │  └─ OneStep.Infrastructure   EF Core, file storage, email, JWT
│  └─ tests/
│     └─ OneStep.UnitTests
├─ frontend/                      Angular application
├─ database/                      hand-written SQL scripts (schema, views, procedures, seed)
└─ docs/
   └─ architecture/               C4 model diagrams
```

## Architecture

The backend follows clean architecture: dependencies point inward, toward `Domain`, which has no dependencies of its own. Architecture diagrams follow the [C4 model](https://c4model.com) and are in [`docs/architecture`](docs/architecture).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server 2025 Developer edition and SQL Server Management Studio
- [Node.js](https://nodejs.org) 24 LTS or newer
- Angular CLI 22: `npm install -g @angular/cli@22`
- EF Core tools: `dotnet tool install --global dotnet-ef`

## Getting started

### Backend

```bash
cd backend
dotnet build
dotnet test
dotnet run --project src/OneStep.Api
```

### Frontend

```bash
cd frontend
npm install
ng serve
```

Then open http://localhost:4200.

### Database

_Coming in the next milestone: connection string setup, migrations and seed data._

## Features

_This section will list the finished features with screenshots._

## API documentation

_Swagger UI will be available at `/swagger` when the API runs in Development._

## Author

Gio — IT Step Academy, Tbilisi
