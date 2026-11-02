# ADR 0008: Cancellation Tokens Through Every Layer

## Context

A caller can abandon a request, and a host can stop while work is in flight. Work that keeps running after that wastes database and network time. A token only cancels what receives it, so one layer that drops it stops cancellation from reaching everything below.

## Decision

- API actions take a `CancellationToken` parameter, which ASP.NET Core binds to the request's aborted token, and pass it to the application service.
- Async methods in Core end with a `CancellationToken cancellationToken = default` parameter. This covers application services, repositories, integration clients, and the unit of work.
- Each method passes its token to every async call it makes, down to the RavenDB client and HTTP calls.

## Consequences

- A cancelled request stops its RavenDB and HTTP calls instead of running to completion.
- Because the parameter has a default, a caller that omits the token also compiles.

## Reference implementation

- The token threaded from controller to repository: [`VendorsController.Activate`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Controllers/VendorsController.cs) through [`VendorsApplicationService.ActivateAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Core/ApplicationServices/Implementations/VendorsApplicationService.cs) to [`VendorRepository.GetAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Repositories/Implementations/VendorRepository.cs): `await vendorRepository.GetAsync(id, cancellationToken)`.
