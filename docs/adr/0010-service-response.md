# ADR 0010: Application Services Return ServiceResponse for Expected Failures

## Context

A call into an application service can succeed, fail because a rule was not met, or hit an unexpected exception. A caller should handle the first two without catching exceptions, and an API client should see every failure in the same shape.

## Decision

- Application services return `ServiceResponse<T>` ([`ApplicationServices/Abstractions/ServiceResponse.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/ApplicationServices/Abstractions/ServiceResponse.cs)). `Succeed(data)` carries the data. `Fail(type, errors)` carries an `ErrorType` and one or more `DomainError`s, each a code and a message. `Fail` also takes a `DomainErrorEnvelope`.
- `T` is a response DTO from `Contracts/Responses`, never an entity ([ADR 0003](0003-domain-driven-design.md)).
- `FailureType` is the `ErrorType` passed to `Fail`. Nothing derives it from the errors. The categories are the ones `DomainException` uses: Validation, NotFound, Unauthorized, Forbidden, BusinessRuleViolation, and Unexpected.
- A missing entity or a broken business rule is a returned failure, not an exception. An unexpected failure stays an exception, including one a service catches, logs, and rethrows. `DomainException` carries the same category and errors for code that cannot return a response, such as an entity method.
- Errors are defined as static methods on a class named for the domain concept, such as `VendorErrors.NotFound(id)` in [`Domain/Exceptions/VendorErrors.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Domain/Exceptions/VendorErrors.cs).
- Controllers inherit `ApiControllerBase`. An action validates the request with FluentValidation ([ADR 0005](0005-fluent-validation.md)) and returns `InvalidRequest(validation)` when it fails. Otherwise it calls the service and returns `ToResult(response)`, or `ToPagedResult(response)` for a page of a list ([ADR 0014](0014-pagination.md)).
- `ToResult` answers a success with 200 and an `ApiResponse<T>`: `data`, and `metadata` holding `traceId` and a UTC `timestamp`. `ToPagedResult` answers with a `PagedApiResponse<T>`, which adds `pagination` beside `data`. The body has no `success`, status, or header members, because HTTP carries those. A failure becomes a problem response ([ADR 0011](0011-problem-details.md)).
- Metadata beyond `traceId` and `timestamp` is not decided.

## Consequences

- A caller gets one shape for every expected failure and does not catch exceptions to tell them apart.
- Every application service method returns `ServiceResponse<T>`, including one that has no failure path yet.
- A failure has one category. A use case that finds problems in several categories must choose one.
- Nothing checks that the errors match the category the service passes.

## Reference implementation

- Type: [`ServiceResponse.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/ApplicationServices/Abstractions/ServiceResponse.cs)
- A success and a `NotFound` failure side by side, in [`VendorsApplicationService.ActivateAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Core/ApplicationServices/Implementations/VendorsApplicationService.cs): `ServiceResponse<VendorResponseDto>.Fail(ErrorType.NotFound, VendorErrors.NotFound(id))` and `ServiceResponse<VendorResponseDto>.Succeed(vendor.ToResponseDto())`.
- An error definition: [`VendorErrors.NotFound`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Domain/Exceptions/VendorErrors.cs)
- Turning the response into an HTTP result: [`ApiControllerBase.ToResult`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Controllers/ApiControllerBase.cs). A page of a list uses `ToPagedResult` in the same file ([ADR 0014](0014-pagination.md)).
