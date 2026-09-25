# Inventory Management System

A full-stack inventory management application for managing products, suppliers,
stock movements, purchase orders, and operational audit history. The project
uses ASP.NET Core and Angular in a Clean Architecture monorepo.

## Features

- JWT authentication with role-based authorization for Admin, Manager, and Staff users.
- Product, category, supplier, and user management.
- Inventory stock-in, stock-out, and adjustment workflows.
- Immutable inventory movement history, including the previous and resulting stock levels.
- Purchase-order creation, submission, receiving, and cancellation workflows.
- Audit logs for authentication and inventory operations.
- Dashboard reporting for stock activity, low-stock items, category inventory, and top-moving products.

## Technology Stack

- Backend: ASP.NET Core Web API, .NET 10, Entity Framework Core, ASP.NET Core Identity, JWT.
- Database: MySQL.
- Frontend: Angular 22, Angular Material, Chart.js.
- Testing: xUnit and Angular ESLint.

## Architecture

```text
Angular SPA
    |
ASP.NET Core Web API
    |
Application / Domain / Infrastructure
    |
Entity Framework Core + MySQL
```

The backend follows Clean Architecture:

- `Domain`: entities, enums, and core business rules.
- `Application`: use cases, DTOs, validation, interfaces, and service orchestration.
- `Infrastructure`: EF Core persistence, repositories, authentication, and migrations.
- `Api`: controllers, middleware, dependency injection, and HTTP contracts.

## Repository Structure

```text
src/
  backend/
    Api/                  ASP.NET Core API host
    Application/          Application services and DTOs
    Domain/               Business entities and rules
    Infrastructure/       EF Core, MySQL, repositories, authentication
  frontend/               Angular application
tests/
  backend/Unit/           Backend unit tests
docs/                     Architecture, API, database, and design documentation
```

## Prerequisites

- .NET 10 SDK
- Node.js and npm
- MySQL Server

## Run Locally

### 1. Configure and run the API

The API uses user secrets so database and JWT credentials are not committed to source control.

```powershell
cd src/backend/Api

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=inventory_management_system_dev;User=root;Password=YOUR_PASSWORD;"
dotnet user-secrets set "Jwt:SigningKey" "replace-this-with-a-long-random-development-key"
dotnet user-secrets set "SeedAdmin:Email" "admin@example.com"
dotnet user-secrets set "SeedAdmin:Password" "Admin123"

dotnet ef database update --project ../Infrastructure/Infrastructure.csproj --startup-project .
dotnet run -- --provision-admin
dotnet run
```

The API runs at `http://localhost:5253`.

On a new local database, run the commands in this order:

1. `dotnet ef database update` applies the committed migrations.
2. `dotnet run -- --provision-admin` creates the configured initial administrator if no users exist, then exits.
3. `dotnet run` starts the API without changing the database schema or creating users.

For a database that already has users, do not run `--provision-admin`; it will exit without creating an account.

### 2. Run the Angular application

Open a second terminal:

```powershell
cd src/frontend
npm install
npm start
```

Open `http://localhost:4200` in your browser. The Angular development server proxies `/api` requests to the local API.

### Demo Access

Sign in with the email and password you configured through `SeedAdmin:Email` and `SeedAdmin:Password`. Demo credentials are intentionally not committed to the repository.

## Quality Checks

Run backend unit tests from the repository root:

```powershell
dotnet test tests/backend/Unit/Unit.csproj --no-restore
```

Run frontend checks:

```powershell
cd src/frontend
npm run lint
npm run build
```

## Screenshots

Add screenshots of the dashboard, stock movement workflow, purchase orders, and audit logs under `docs/images/`, then reference them here. For example:

```md
![Dashboard](docs/images/dashboard.png)
```

## Live Demo

Deployment is not configured yet. Add the deployed application URL here once it is available.

## Documentation

- [Software Design Document](docs/sdd.md)
- [System Architecture](docs/architecture.md)
- [API Design](docs/api-design.md)
- [Database Design](docs/database.md)
