# ADR 0006: RavenDB for Persistence

## Context

RavenDB is a document database with no fixed schema. Its .NET client provides a session that tracks changes to loaded documents and saves them in one transaction.

## Decision

- Services store their data in RavenDB and access it through the RavenDB .NET client (`RavenDB.Client`). EF Core is not used.
- `AddCore()` registers `IDocumentStore` as a singleton and `IAsyncDocumentSession` as scoped, in [`DependencyInjection/RavenDbRegistration.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/DependencyInjection/RavenDbRegistration.cs) ([ADR 0001](0001-host-registration-pattern.md)). Every host that calls `AddCore()` gets the same registration.
- The document store reads `Urls` and `Database` from the `RavenDb` configuration section.
- The document store sets `IdentityPartsSeparator` to `-`, so generated ids look like `vendors-1-A` and fit a single route segment.
- The host does not create or ensure the database at startup. Creating a database is a server-admin operation (`/admin/databases`), which a secured cluster allows only for Operator or ClusterAdmin certificates. The database is provisioned outside the application — by an operator, deployment step, or local RavenDB studio — before the host runs against it.
- The solution has no database migrations project.

## Consequences

- Repositories in the same scope share one session, so a use case's changes can be saved together in one transaction.
- Hosts start with a database-user certificate; they do not need Operator or ClusterAdmin rights.
- A mistyped `RavenDb:Database` fails when the first session uses that database, instead of creating a new empty one.
- Startup no longer probes RavenDB with admin APIs on every cold start (including Azure Functions).

## Reference implementation

- Registration, including the id separator: [`RavenDbRegistration.AddCoreRavenDb`](../../src/api/PartsTrackerErp.GoldenPattern.Core/DependencyInjection/RavenDbRegistration.cs)
