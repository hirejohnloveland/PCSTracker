# ADR 0005: FluentValidation for Boundary Input Validation

## Context

Request DTOs need input checks (required fields, ranges, formats) before a use case runs. DataAnnotations couple validation to the DTO type and are awkward to unit test in isolation. Domain invariants belong on entities and value objects, not on transport shapes.

## Decision

- Boundary/input validation uses FluentValidation.
- Validators live in [`Core/Validation/Validators`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Validation/Validators) and target request contracts in [`Contracts/Requests`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Contracts/Requests).
- Validators are registered in `AddCore()` via `AddValidatorsFromAssemblyContaining`.
- Validation is done on both sides. The API rejects bad input with an error entry for every failing field, each with a message and a `pointer` ([ADR 0011](0011-problem-details.md)). The front end validates input against its data model before it sends a request ([ADR 0019](0019-front-end-error-handling.md)). Neither side relies on the other.
- Domain rules stay in the domain. FluentValidation does not replace entity or value-object invariants.
- A request DTO property the caller must send is declared `required`, which is what lists it as required in the OpenAPI document ([ADR 0015](0015-openapi-responses.md)). A property the caller may leave out is not `required`. The validator decides whether a value is acceptable: not empty, in range, in format.

## Consequences

- Validators are plain classes that can be unit tested without hosting ASP.NET Core.
- Every host that calls `AddCore()` gets the same validators.
- Controllers (or filters) resolve `IValidator<T>` and return validation failures as HTTP 400 responses.
- A body that leaves out a `required` property still binds, with the property unset. `AddApiErrorHandling` turns off System.Text.Json's required check and MVC's implicit required check, so the validator reports the missing property at its own pointer, such as `#/name`, together with every other failure. A validator must therefore reject an unset value for each property the DTO declares `required`, as `NotEmpty` does for `Name`.

## Reference implementation

- A validator's rule, in [`VendorDtoValidator`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Validation/Validators/VendorDtoValidator.cs): `RuleFor(x => x.Name).NotEmpty().WithMessage("Vendor name is required.");`
- A property the caller must send, in [`VendorDto`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Contracts/Requests/VendorDto.cs): `public required string Name { get; set; }`
- Registration: [`CoreRegistration.AddCore`](../../src/api/PartsTrackerErp.GoldenPattern.Core/DependencyInjection/CoreRegistration.cs) calls `AddValidatorsFromAssemblyContaining<VendorInvoiceDtoValidator>()`.
