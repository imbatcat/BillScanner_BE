# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run the API
dotnet run --project BillScanner

# Run all tests
dotnet test

# Run a single test by name
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Run tests in a specific class
dotnet test --filter "ClassName~CreateBillHandlerTests"

# Add EF Core migration
dotnet ef migrations add <MigrationName> --project Infrastructure --startup-project BillScanner

# Apply migrations
dotnet ef database update --project Infrastructure --startup-project BillScanner
```

Swagger UI is available at `/swagger` when running in Development or Test environments.

## Architecture

Five-project solution (`BillScanner.slnx`):

- **`BillScanner`** — ASP.NET Core Web API. Presentation layer: controllers, middleware, DI wiring (`Program.cs`), JWT auth, OpenAPI config.
- **`Business`** — Application logic. MediatR command/query handlers, builders, specifications, and service interfaces.
- **`Domain`** — Entities, domain events, and exceptions. No dependencies on other layers.
- **`Infrastructure`** — EF Core (`BillScannerDbContext`), repositories, and external service implementations (Cloudinary, Azure Document Intelligence, Redis).
- **`Test`** — xUnit tests split into `Unit/` and `Integration/` subdirectories.

### CQRS / MediatR

All business operations go through MediatR. Controllers call `_mediator.Send(command)`. Handlers live in `Business/Handlers/<Feature>/<Action>/` alongside their DTOs in a `Dto/` subfolder.

### Repository / Unit of Work

`IGenericRepository<T>` provides typed CRUD + specification-based queries. `IUnitOfWork` wraps multiple repositories and exposes `CommitAsync()`. Access via `unitOfWork.Repository<Entity>()`.

### Specification Pattern

Query filtering is done via `BaseSpecification<T>` subclasses (`Business/Specifications/`). The `SpecificationEvaluator` applies them to `IQueryable<T>` in the repository.

### Builder Pattern

Entity construction uses typed builders (e.g., `IBillBuilder`, `IBillItemBuilder`) resolved via `IBuilderFactory.Builder<T>()`. Builders are registered via Scrutor assembly scanning of `IBuilderMarker`. The `BillBuilder` also tracks OCR extraction accuracy — when a user overrides an extracted value, the corresponding `Is*Correct` flag on `BillExtractionResult` is set to `false`.

### Core Bill Flow

1. **Image upload**: client uploads to Cloudinary, which fires a webhook (`POST /webhooks/file-uploaded`).
2. **OCR**: webhook triggers `FileUploadedWebhookHandler` → publishes `ImageUploadedEvent` → `ImageUploadedProcessImageHandler` calls Azure Document Intelligence via `IImageExtractionService`.
3. **Cache**: `ProcessImageHandler` stores the `ImageProcessResult` in Redis under a key scoped to `(userId, imageUrl)` with a configurable TTL (`BusinessSettings.CacheExpirationTimeInMinutes`, default 10 min).
4. **Validation**: extracted totals are cross-checked against line items; if they don't reconcile, a `ScanRetryRequiredException` is thrown.
5. **Commit**: `POST /bills` → `CreateBillHandler` retrieves the cached result, runs it through `BillBuilder.FromProcessResult()`, applies `UserEditsDto`, then persists `Bill` + `BillExtractionResult` via `UnitOfWork`.

### DI Registration Conventions

- **Infrastructure services**: implement `IScopedService`, `ITransientService`, or `ISingletonService` marker interfaces — Scrutor auto-registers them in `AddInfrastructure()`.
- **Settings classes**: any class in the Infrastructure assembly implementing `IAppSettings` is automatically bound to the config section matching its class name (e.g., `RedisSettings` binds to `"RedisSettings"` in appsettings).
- **Builders**: implement `IBuilderMarker` and are registered as transient via Scrutor in `AddApplications()`.

### Testing

Integration tests use `CustomWebApplicationFactory` (xUnit `IAsyncLifetime`) which spins up real PostgreSQL and Redis via Testcontainers. **Respawn** resets the database between tests. All integration tests inherit from `BaseTest` and must be in the `"BillScannerTestCollection"` collection.

Unit tests mock dependencies directly — no containers involved.

The `Test` project shares `appsettings.json`, `appsettings.Development.json`, and `appsettings.Test.json` from the `BillScanner` project (copied on build).
