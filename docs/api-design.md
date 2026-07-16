# API Design Guidelines

## Scope
This document defines API design conventions for the Inventory Management System backend.

## Design Principles
- Resource-oriented endpoints with consistent naming.
- Predictable status codes and error payloads.
- Backward-compatible evolution through versioning discipline.
- Validation and domain constraints enforced server-side.

## Base Conventions
- Base path: /api
- Naming: plural nouns for collections (for example, /api/products).
- JSON: camelCase payload properties.
- Time format: ISO 8601 in UTC.

## Versioning Strategy
- Start with unversioned internal API during early milestones.
- Introduce explicit versioning before external consumers onboard.
- Preferred approach: URL versioning (/api/v1/...) with clear deprecation policy.

## Response Patterns

### Success
- 200 OK: successful read/update/delete with payload when applicable.
- 201 Created: successful resource creation with location metadata.
- 204 No Content: successful operation with no response body.

### Client Errors
- 400 Bad Request: malformed payload or validation failure.
- 401 Unauthorized: missing/invalid authentication token.
- 403 Forbidden: authenticated but insufficient permissions.
- 404 Not Found: resource does not exist.
- 409 Conflict: state conflict (for example duplicate unique value).

### Server Errors
- 500 Internal Server Error: unexpected exception.

## Standard Error Contract
Use a consistent error body shape for all non-success responses:
- traceId: request correlation identifier.
- code: stable machine-readable error code.
- message: human-readable summary.
- details: optional list of field or domain validation issues.

## Pagination, Filtering, Sorting
- Pagination query params: page, pageSize.
- Filtering query params: explicit fields (for example categoryId, supplierId, isActive).
- Sorting query param: sortBy with optional direction (asc/desc).
- Include total count metadata for list endpoints.

## Idempotency and Concurrency
- GET, PUT, DELETE should remain idempotent.
- Consider optimistic concurrency tokens for update-heavy entities.
- Document conflict semantics clearly for stock updates and receiving workflows.

## Security Requirements
- Require authentication for non-public endpoints.
- Enforce role/policy checks per operation.
- Validate and sanitize all inbound data.
- Avoid leaking internal exception details in responses.

## Initial Endpoint Candidates
- Health
  - GET /api/health
- Products
  - GET /api/products
  - GET /api/products/{id}
  - POST /api/products
  - PUT /api/products/{id}
  - DELETE /api/products/{id}
- Categories
  - GET /api/categories
  - POST /api/categories
- Suppliers
  - GET /api/suppliers
  - POST /api/suppliers
- Stock Movements
  - GET /api/stock-movements
  - POST /api/stock-movements

## Testing Expectations
- Unit tests for validators and application services.
- Integration tests for endpoint behavior and persistence outcomes.
- Contract checks for response schemas and status semantics.
