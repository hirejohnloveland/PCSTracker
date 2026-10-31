# ADR 0006: RavenDB for Persistence

## Context

RavenDB is a document database with no fixed schema. Its .NET client provides a session that tracks changes to loaded documents and saves them in one transaction.

## Decision

- Services store their data in RavenDB and access it through the RavenDB .NET client (`RavenDB.Client`). EF Core is not used.
- `AddCore()` registers `IDocumentStore` as a singleton and `IAsyncDocumentSession` as scoped, in [`DependencyInjection/RavenDbRegistration.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/DependencyInjection/RavenDbRegistration.cs) ([ADR 0001](0001-host-registration-pattern.md)). Every host that calls `AddCore()` gets the same registration.
- The document store reads `Urls` and `Database` from the `RavenDb` configuration section.
- At startup, a hosted service registered by `AddCore()` creates the database if it does not exist ([`Infrastructure/Persistence/RavenDbDatabaseInitializer.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Infrastructure/Persistence/RavenDbDatabaseInitializer.cs)).
- The solution has no database migrations project.

## Consequences

- Repositories in the same scope share one session, so a use case's changes can be saved together in one transaction.
- A host does not start unless RavenDB is reachable.
- A mistyped `RavenDb:Database` creates a new empty database instead of failing.

## Reference implementation

- Registration: [`RavenDbRegistration.AddCoreRavenDb`](../../src/api/PartsTrackerErp.GoldenPattern.Core/DependencyInjection/RavenDbRegistration.cs)
- Startup database creation: [`RavenDbDatabaseInitializer`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Infrastructure/Persistence/RavenDbDatabaseInitializer.cs)
