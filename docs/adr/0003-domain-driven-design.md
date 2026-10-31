# ADR 0003: Domain-Driven Design with Repositories and Unit of Work

## Context

Earlier services used anemic domain models: entities held data and services held the logic, so services grew too large. Their repositories were hard to group into a single transaction.

## Decision

- Entities carry behavior. Methods on the entity change its state and enforce its rules.
- Domain services hold rules that don't belong to a single entity or value object.
- Application services coordinate use cases and contain no business rules.
- Repositories load entities through RavenDB sessions and don't call `SaveChangesAsync`.
- A unit of work commits each use case's changes in one transaction. It is maintained through persistence handlers and RavenDB sessions.

## Consequences

- Services stay small because business rules live on entities and domain services.
- Each use case commits once, so its changes succeed or fail together.
- Entities take more design work: state changes go through methods, and RavenDB's serialization has to support that.
