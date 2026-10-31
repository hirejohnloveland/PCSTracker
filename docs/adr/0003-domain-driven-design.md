# ADR 0003: Domain-Driven Design with Repositories and Unit of Work

## Context

Earlier services used anemic domain models: entities held data and services held the logic, so services grew too large. Their repositories were hard to group into a single transaction.

## Decision

- Entities carry behavior. Methods on the entity change its state and enforce its rules.
- Domain services hold rules that don't belong to a single entity or value object.
- Application services coordinate use cases and contain no business rules.
- Entities do not leave the application service. The service maps the entity to a response DTO in `Contracts/Responses` before it returns, so an entity never reaches the HTTP surface, whether in a response, in a wrapper such as `ServiceResponse<T>`, in a domain error or exception, or in any other way.
- Repositories load entities through RavenDB sessions and don't call `SaveChangesAsync`.
- A unit of work commits each use case's changes in one transaction. It is maintained through persistence handlers and RavenDB sessions.

## Consequences

- Services stay small because business rules live on entities and domain services.
- Each use case commits once, so its changes succeed or fail together.
- Entities take more design work: state changes go through methods, and RavenDB's serialization has to support that.

## Reference implementation

- An entity enforcing its own state change: `vendor.Activate()` in [`VendorsApplicationService.ActivateAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Core/ApplicationServices/Implementations/VendorsApplicationService.cs), defined on [`Vendor`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Domain/Entities/Vendor.cs).
- An entity mapped to a response before it leaves the service: `vendor.ToResponseDto()` in [`VendorsApplicationService.ActivateAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Core/ApplicationServices/Implementations/VendorsApplicationService.cs), defined in [`VendorMappingExtensions`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Mapping/VendorMappingExtensions.cs) and returning [`VendorResponseDto`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Contracts/Responses/VendorResponseDto.cs).
- A repository that loads but never saves: [`IVendorRepository`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Repositories/Abstractions/IVendorRepository.cs) / [`VendorRepository`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Repositories/Implementations/VendorRepository.cs)
- The unit of work that commits the use case: [`IUnitOfWork`](../../src/api/PartsTrackerErp.GoldenPattern.Core/ApplicationServices/Abstractions/IUnitOfWork.cs) / [`RavenUnitOfWork`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Infrastructure/Persistence/RavenUnitOfWork.cs)
