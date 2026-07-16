# System Architecture

## Overview
The project follows a clean, layered monorepo architecture:
- Backend: ASP.NET Core with Domain, Application, Infrastructure, and API layers.
- Frontend: Angular SPA organized into core/shared/features/layouts.
- Tests: backend-focused unit test project with room for frontend and integration expansion.

## Repository Structure
- src/backend
  - Api: composition root, HTTP endpoints, middleware, API contracts.
  - Application: use cases, DTO orchestration, validation, mapping, service contracts.
  - Domain: entities, enums, domain rules, core interfaces.
  - Infrastructure: persistence, repository implementations, external integrations.
- src/frontend
  - Angular app shell and feature modules.
- tests/backend/Unit
  - Unit tests for domain/application behavior.

## Layer Responsibilities

### Domain
- Owns business entities and invariants.
- Has no dependency on framework-specific concerns.
- Defines core interfaces needed by upper layers.

### Application
- Implements use-case orchestration.
- Coordinates domain operations through interfaces.
- Contains validation, mapping, and application exceptions.

### Infrastructure
- Implements persistence and external service contracts.
- Contains EF Core DbContext, configurations, migrations, repositories.
- Is wired via dependency injection into API.

### API
- Hosts HTTP transport concerns.
- Configures dependency injection, middleware, and endpoint routing.
- Translates application results/exceptions into HTTP responses.

## Cross-Cutting Concerns
- Error handling: centralized global exception middleware.
- Configuration: environment-specific appsettings and user secrets.
- API discoverability: Swagger/OpenAPI for development.
- Security: planned token-based authentication and role authorization.
- CORS: configured for frontend-backend communication.

## Data Flow
1. Frontend sends HTTP request to API endpoint.
2. API controller delegates to application service/use case.
3. Application coordinates domain logic and repository operations.
4. Infrastructure persists/reads data from MySQL via EF Core.
5. Application returns DTO/result; API maps to consistent response format.

## Architectural Decisions
- Clean architecture boundaries are preserved to keep domain logic testable and framework-independent.
- Monorepo chosen for coordinated backend/frontend evolution and shared delivery cadence.
- EF Core selected for persistence productivity with migration support.

## Quality Attributes
- Maintainability: clear boundaries and dependency direction.
- Testability: domain/application logic isolated from transport and storage.
- Reliability: centralized exception handling and transactional persistence patterns.
- Evolvability: feature-oriented frontend structure and layered backend contracts.

## Near-Term Architecture Work
- Add authentication/authorization pipeline (JWT + policies).
- Introduce integration test project for API + persistence behavior.
- Define standardized problem details/response contract across endpoints.
- Add observability baseline (structured logs, tracing correlation).
