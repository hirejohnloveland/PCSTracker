# ADR 0001: Host Registration Pattern

## Context

Service registrations in `Program.cs` grow without bound and cannot be reused. Azure Functions hosts will later share the Core library with the API.

## Decision

- Each project exposes one public entry point in `DependencyInjection.cs`: an extension method on `IHostApplicationBuilder` that returns the builder (`AddCore()`, `AddApi()`).
- Host-only concerns are `internal static` classes, one per file, in the host's `Registration/` folder. The entry point calls them.
- `Program.cs` calls entry points only. The middleware pipeline stays in `Program.cs` so its order is visible.
- A Functions host calls `AddCore()` and adds its own host-only registrations.

## Consequences

- `Program.cs` stays short.
- Core registrations are written once and reused by every host.
- Core depends on `Microsoft.Extensions.Hosting.Abstractions`.
