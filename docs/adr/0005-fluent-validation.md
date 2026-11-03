# ADR 0005: FluentValidation for Boundary Input Validation

## Context

Request DTOs need input checks (required fields, ranges, formats) before a use case runs. DataAnnotations couple validation to the DTO type and are awkward to unit test in isolation. Domain invariants belong on entities and value objects, not on transport shapes.

## Decision

- Boundary/input validation uses FluentValidation.
- Validators live in [`Core/Validation/Validators`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Validation/Validators) and target request contracts in [`Contracts/Requests`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Contracts/Requests).
- Validators are registered in `AddCore()` via `AddValidatorsFromAssemblyContaining`.
- Domain rules stay in the domain. FluentValidation does not replace entity or value-object invariants.
- A request DTO property the caller must send is declared `required`. The OpenAPI document does not read validator rules, so `required` is what lists the property as required there ([ADR 0015](0015-openapi-responses.md)). `required` says the property must be present. The validator still decides whether its value is acceptable: not empty, in range, in format. A property the caller may leave out is not `required`.

## Consequences

- Validators are plain classes that can be unit tested without hosting ASP.NET Core.
- Every host that calls `AddCore()` gets the same validators.
- Controllers (or filters) resolve `IValidator<T>` and return validation failures as HTTP 400 responses.
- A body that leaves out a `required` property is rejected while the body is read, before a validator runs. It is answered as a body MVC cannot bind ([ADR 0011](0011-problem-details.md)): no entry names the missing property, and the validator's message for it is not sent.

## Reference implementation

- A validator's rule, in [`CreateVendorDtoValidator`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Validation/Validators/CreateVendorDtoValidator.cs): `RuleFor(x => x.Name).NotEmpty().WithMessage("Vendor name is required.");`
- A property the caller must send, in [`CreateVendorDto`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Contracts/Requests/CreateVendorDto.cs): `public required string Name { get; set; }`
- Registration: [`CoreRegistration.AddCore`](../../src/api/PartsTrackerErp.GoldenPattern.Core/DependencyInjection/CoreRegistration.cs) calls `AddValidatorsFromAssemblyContaining<VendorInvoiceDtoValidator>()`.
