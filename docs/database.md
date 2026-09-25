# Database Design and Operations

## Scope
This document captures database conventions and operational guidance for MySQL in the Inventory Management System.

## Technology Baseline
- Engine: MySQL
- ORM: Entity Framework Core
- Migration ownership: Infrastructure project
- Runtime connection: configured via user secrets or environment variables

## Database Tables

The database currently contains the following tables:

### Core Application Tables
- Products
- Categories
- Suppliers
- InventoryMovements
- PurchaseOrders
- PurchaseOrderItems
- Users
- Roles
- AuditLogs
- RefreshTokens

### ASP.NET Core Identity Tables
- AspNetRoleClaims
- AspNetUserClaims
- AspNetUserLogins
- AspNetUserRoles
- AspNetUserTokens

### Entity Framework Core Tables
- __EFMigrationsHistory

## Modeling Conventions
- Use surrogate primary keys for internal identity.
- Define explicit foreign keys and required relationships.
- Enforce uniqueness where domain requires it (for example SKU).
- Prefer explicit decimal precision for monetary/quantity fields.
- Store timestamps in UTC.

## Transaction and Consistency Rules
- Stock adjustments and receiving workflows must be transactional.
- Reject operations that would violate non-negative stock constraints unless explicitly allowed by policy.
- Preserve immutable movement history for traceability.

## Indexing Guidance
- Index common lookup columns (for example SKU, categoryId, supplierId).
- Add composite indexes for frequent filtered queries.
- Review and tune indexes using real query patterns after feature rollout.

## Migration Workflow
1. Create model/configuration changes in Infrastructure persistence layer.
2. Generate migration with clear naming based on intent.
3. Review generated SQL impact before applying.
4. Apply migration in local/dev environments with an explicit EF Core command.
5. Commit migration artifacts with corresponding code changes.

## Database Deployment

The API does not apply migrations during startup. Database changes are a separate deployment step and must complete before the new API version is released.

1. Back up the target database and verify the backup can be restored.
2. Review the pending migration SQL and the target migration.
3. Run the migration step once with a deployment database account that has schema-modification privileges.
4. Verify the expected entries exist in `__EFMigrationsHistory`.
5. Start or roll out the API using its restricted runtime database account.

For a new installation only, provision the first administrator as a separate one-shot operation after migrations have been applied:

```powershell
dotnet run --project src/backend/Api -- --provision-admin
```

Supply `SeedAdmin:Email` and `SeedAdmin:Password` through user secrets for local development or a managed secret store for production. Do not place administrator credentials in source-controlled configuration. The command exits without creating a user when the database already contains users.

## Database Accounts

Use separate database accounts for deployment and normal API operation:

- The deployment account can apply approved migrations and update `__EFMigrationsHistory`.
- The API runtime account requires only application data permissions, such as `SELECT`, `INSERT`, `UPDATE`, and `DELETE`.

Do not grant the API runtime account schema privileges such as `CREATE`, `ALTER`, `DROP`, or `INDEX`.

## Rollback and Verification

Before a migration, take a verified backup. If the migration or release fails, stop the rollout and restore that backup when schema or data rollback is needed. Deploy the previous API version only after confirming it is compatible with the restored schema.

After deployment, verify the applied migration history, API startup with the runtime account, administrator sign-in, and role-protected endpoints.

## Environment Configuration
- Local/dev credentials must not be stored in source-controlled appsettings.
- Use dotnet user-secrets or environment variables for connection strings.
- Keep production secrets in managed secret stores.

## Backup and Recovery (Planned)
- Define backup retention policy by environment.
- Validate restore procedures regularly in non-production.
- Document RPO/RTO targets and ownership.

## Data Governance
- Minimize sensitive data collection.
- Add clear retention and archival rules for logs and audit trails.
- Define deletion strategy (hard delete vs soft delete) per aggregate.

## Observability for Data Layer
- Enable structured logs around critical database operations.
- Capture slow query patterns during performance testing.
- Add health checks that verify basic database connectivity.
