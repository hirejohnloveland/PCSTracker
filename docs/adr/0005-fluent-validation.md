# ADR 0005: FluentValidation for Boundary Input Validation

## Context

Request DTOs need input checks (required fields, ranges, formats) before a use case runs. DataAnnotations couple validation to the DTO type and are awkward to unit test in isolation. Domain invariants belong on entities and value objects, not on transport shapes.

## Decision

- Boundary/input validation uses FluentValidation.
- Validators live in [`Core/Validation/Validators`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Validation/Validators) and target request contracts in [`Contracts/Requests`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Contracts/Requests).
- Validators are registered in `AddCore()` via `AddValidatorsFromAssemblyContaining`.
- Domain rules stay in the domain. FluentValidation does not replace entity or value-object invariants.

## Consequences

- Validators are plain classes that can be unit tested without hosting ASP.NET Core.
- Every host that calls `AddCore()` gets the same validators.
- Controllers (or filters) resolve `IValidator<T>` and return validation failures as HTTP 400 responses.

## Reference implementation

- A validator's rule, in [`CreateVendorDtoValidator`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Validation/Validators/CreateVendorDtoValidator.cs): `RuleFor(x => x.Name).NotEmpty().WithMessage("Vendor name is required.");`
- Registration: [`CoreRegistration.AddCore`](../../src/api/PartsTrackerErp.GoldenPattern.Core/DependencyInjection/CoreRegistration.cs) calls `AddValidatorsFromAssemblyContaining<VendorInvoiceDtoValidator>()`.
