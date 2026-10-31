# ADR 0004: Interfaces Live Next to Their Implementations

## Context

Microsoft's DDD guidance puts repository interfaces in the domain layer and their implementations in infrastructure, so the domain never depends on infrastructure. Core is a single assembly ([ADR 0002](0002-single-core-library.md)), so where an interface lives doesn't change what can reference it. Keeping an interface next to its implementation makes both easier to find.

## Decision

- Repository interfaces and implementations live together in `Repositories/Abstractions` and `Repositories/Implementations`.
- Integration client interfaces and implementations live together in `Integrations/Abstractions` and `Integrations/Implementations`.
- These interfaces are not moved into `Domain` or `ApplicationServices` to match textbook layering.

## Consequences

- Each interface and its implementation are found in one place.
- Application services reference the `Repositories` and `Integrations` namespaces directly, so architecture tests must allow those references.
- Developers who know Microsoft's DDD guidance will look for repository interfaces in the domain layer first.
