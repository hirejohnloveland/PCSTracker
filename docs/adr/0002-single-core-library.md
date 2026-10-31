# ADR 0002: Single Core Library with Folder Boundaries

## Context

The business code is split into DDD layers: application services, domain, repositories, integrations, and contracts. Putting each layer in its own project, as Microsoft's eShop reference app does, lets the compiler enforce references between layers. Every service cloned from this template would also carry those extra projects.

## Decision

- `PartsTrackerErp.GoldenPattern.Core` is one class library. Each layer is a folder, and namespaces follow folders.
- A new project is added only when a concrete need justifies it.

## Consequences

- Services built from the template have fewer projects and references to maintain.
- The compiler does not enforce layer boundaries, and `internal` does not separate layers inside one assembly. Boundary checks have to come from architecture tests or analyzers.
