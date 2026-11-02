# ADR 0006: RavenDB for Persistence

## Context

RavenDB is a document database with no fixed schema. Its .NET client provides a session that tracks changes to loaded documents and saves them in one transaction.

## Decision

- Services store their data in RavenDB and access it through the RavenDB .NET client (`RavenDB.Client`). EF Core is not used.
- `AddCore()` registers `IDocumentStore` as a singleton and `IAsyncDocumentSession` as scoped, in [`DependencyInjection/RavenDbRegistration.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/DependencyInjection/RavenDbRegistration.cs) ([ADR 0001](0001-host-registration-pattern.md)). Every host that calls `AddCore()` gets the same registration.
- The document store reads `Urls` and `Database` from the `RavenDb` configuration section.
- The document store sets `IdentityPartsSeparator` to `-`, so generated ids look like `vendors-1-A` and fit a single route segment.


## Consequences

- Repositories in the same scope share one session, so a use case's changes can be saved together in one transaction.
- Hosts start with a database-user certificate; they do not need Operator or ClusterAdmin rights.
- A mistyped `RavenDb:Database` fails when the first session uses that database, instead of creating a new empty one.


## Reference implementation

- Registration, including the id separator: [`RavenDbRegistration.AddCoreRavenDb`](../../src/api/PartsTrackerErp.GoldenPattern.Core/DependencyInjection/RavenDbRegistration.cs)
