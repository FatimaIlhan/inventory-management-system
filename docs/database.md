# Database Design and Operations

## Scope
This document captures database conventions and operational guidance for MySQL in the Inventory Management System.

## Technology Baseline
- Engine: MySQL
- ORM: Entity Framework Core
- Migration ownership: Infrastructure project
- Runtime connection: configured via user secrets or environment variables

## Core Entities (Planned)
- Products
- Categories
- Suppliers
- StockMovements
- PurchaseOrders
- PurchaseOrderItems
- Users
- Roles
- AuditLogs

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
4. Apply migration in local/dev environments.
5. Commit migration artifacts with corresponding code changes.

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
