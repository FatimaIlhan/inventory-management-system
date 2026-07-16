# Inventory Management System

Clean monorepo scaffold for an inventory management system.

## Planned Structure

```text
src/
  backend/
    Api/
      Controllers/
      DTOs/
    Application/
      Services/
      Interfaces/
    Domain/
      Entities/
      Enums/
    Infrastructure/
      Persistence/
      Repositories/
      Config/
tests/
  backend/
    Unit/
  frontend/
    src/app/
      core/
      shared/
        components/
        pipes/
        directives/
      features/
        auth/
        products/
        categories/
        suppliers/
        stock-movements/
        purchase-orders/
        dashboard/
        audit-logs/
      layouts/
```

## Next Step

Add the actual ASP.NET Core solution and the Angular workspace inside `src/backend` and `src/frontend`.

## Documentation

Project documentation is maintained in the `docs` folder:

- `docs/sdd.md`
- `docs/roadmap.md`
- `docs/architecture.md`
- `docs/api-design.md`
- `docs/database.md`
- `docs/decisions.md`

## Backend MySQL Secrets Setup

Do not store database passwords in `appsettings.json`.

Set the API connection string with user secrets:

```powershell
cd src/backend/Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=inventory_management_system_dev;User=root;Password=YOUR_PASSWORD;"
```

Or set an environment variable:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=inventory_management_system_dev;User=root;Password=YOUR_PASSWORD;"
```
