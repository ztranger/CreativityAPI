# API.ApiService Structure

## Purpose

This document describes the current code organization of `API.ApiService` and related layers after introducing a DB-backed users repository with EF Core + PostgreSQL, while preserving an in-memory provider.

## Solution Projects

- `API.ApiService` - HTTP layer (Minimal API), endpoint mapping, middleware, and DI composition.
- `API.Domain` - domain models and value objects without ASP.NET dependencies.
- `API.Application` - application services and contracts (interfaces for repositories and token generation).
- `API.Infrastructure` - technical implementations (JWT, repository implementations, EF Core persistence, migrations).
- Existing projects `API.Web`, `API.ServiceDefaults`, `API.AppHost`, `API.Tests` remain in place.

## High-Level Dependency Rules

- `API.ApiService -> API.Application`
- `API.ApiService -> API.Infrastructure`
- `API.Application -> API.Domain`
- `API.Infrastructure -> API.Application`
- `API.Infrastructure -> API.Domain`

`API.Domain` should not reference other application projects.

## API.ApiService Folder Structure

```text
API.ApiService/
  Program.cs
  Common/
    Extensions/
      ServiceCollectionExtensions.cs
  Features/
    Auth/
      AuthEndpoints.cs
      AuthContracts.cs
      AuthFeatureRegistration.cs
    Users/
      UsersEndpoints.cs
      UsersContracts.cs
      UsersFeatureRegistration.cs
    Weather/
      WeatherEndpoints.cs
```

### Responsibilities inside `API.ApiService`

- `Program.cs`
  - Keeps only composition root logic:
    - service registration
    - middleware pipeline
    - endpoint mapping calls
- `Features/*/Endpoints`
  - Defines HTTP routes (`MapGroup`, `MapGet`, `MapPost`, etc.).
  - Handles HTTP orchestration and response shaping.
- `Features/*/Contracts`
  - Contains request/response DTOs specific to HTTP API.
- `Features/*/FeatureRegistration`
  - Registers feature dependencies into DI.
  - `UsersFeatureRegistration` selects repository provider (`InMemory` or `Postgres`) from config.
- `Common/Extensions/ServiceCollectionExtensions.cs`
  - Centralized auth registration (`AddJwtAuthentication`).

## Layered Business Structure

### `API.Domain`

```text
API.Domain/
  Users/
    User.cs
```

- Contains domain records:
  - `User`
  - `UserSettings`

### `API.Application`

```text
API.Application/
  Auth/
    AuthService.cs
    IUserTokenService.cs
  Users/
    UsersService.cs
    IUsersRepository.cs
```

- `AuthService`
  - register/verify/refresh use-cases.
- `UsersService`
  - current user read/update and users search/read use-cases.
- Interfaces define dependencies on external details:
  - `IUsersRepository`
  - `IUserTokenService`

### `API.Infrastructure`

```text
API.Infrastructure/
  Auth/
    JwtOptions.cs
    JwtTokenService.cs
  Persistence/
    ApiDbContext.cs
    ApiDbContextDesignTimeFactory.cs
    Configurations/
      UserEntityConfiguration.cs
    Entities/
      UserEntity.cs
    Migrations/
      <generated migration files>
  Users/
    InMemoryUsersRepository.cs
    PostgresUsersRepository.cs
```

- Implements application interfaces:
  - `JwtTokenService : IUserTokenService`
  - `InMemoryUsersRepository : IUsersRepository`
  - `PostgresUsersRepository : IUsersRepository`
- Contains EF Core persistence:
  - `ApiDbContext` and entity configuration for `users`
  - design-time factory for `dotnet ef`
  - migrations for DB schema evolution

## Request Flow

```mermaid
flowchart LR
Client --> ApiServiceEndpoints
ApiServiceEndpoints --> ApplicationServices
ApplicationServices --> IUsersRepository
ApplicationServices --> DomainModels
IUsersRepository -->|"Provider=InMemory"| InMemoryRepo
IUsersRepository -->|"Provider=Postgres"| PostgresRepo
PostgresRepo --> ApiDbContext
ApiDbContext --> PostgreSQL
```

## Runtime Configuration

- Repository provider is selected in API config:
  - `UsersRepository:Provider` = `InMemory` or `Postgres`
- PostgreSQL connection is read from:
  - `ConnectionStrings:Main`
- In local Aspire runs (`API.AppHost`), these values are only forwarded as environment variables to `API.ApiService`.
- Decision logic stays in `API.ApiService`, so behavior is consistent both with and without Aspire.

## How to Extend the API

For a new feature (example: `Chats`):

1. Add `Features/Chats/ChatsEndpoints.cs` in `API.ApiService`.
2. Add `Features/Chats/ChatsContracts.cs` for HTTP DTOs.
3. Add or extend use-cases in `API.Application`.
4. Add repository/service interfaces to `API.Application` when needed.
5. Implement technical details in `API.Infrastructure`.
6. Register feature services in `FeatureRegistration` and map endpoints in `Program.cs`.

## Current Constraints and Notes

- Postgres mode requires a valid `ConnectionStrings:Main`; startup succeeds, but user operations fail if DB auth is invalid.
- In-memory mode remains available for local smoke tests and fallback scenarios.
- API contracts remain stable while persistence is switchable behind `IUsersRepository`.
