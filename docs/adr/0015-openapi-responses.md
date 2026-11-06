# ADR 0015: Actions Declare Their Responses for OpenAPI

## Context

The OpenAPI document is generated from the controllers. MVC takes an action's success body type from its declared return type, which `ActionResult<T>` can carry and `IActionResult` cannot, and declaring any response on an action or its base class switches that inference off. Every action can also fail as a problem ([ADR 0011](0011-problem-details.md)), which the generator cannot know.

A client generated from the document gets the types the schemas describe, so a schema that is looser than the API lets mistakes through that the compiler could have caught: a request missing a property the server requires, or a value the API never sends that the client still has to check for null.

## Decision

- An action in the API project returns `Task<ActionResult<TBody>>`, where `TBody` is `ApiResponse<T>` or `PagedApiResponse<T>` ([ADR 0010](0010-service-response.md)), never `IActionResult`. The protected helpers on `ApiControllerBase` return `ActionResult` or `ActionResult<TBody>`. A private helper returns its concrete type.
- An action declares its success without a type: `[ProducesResponseType(StatusCodes.Status200OK)]`, or `Status201Created` for an action that creates a resource. MVC takes the body type from `ActionResult<TBody>`. The success is always declared, because declaring any response switches off the success MVC would infer.
- `ApiControllerBase` declares the six failure responses, one for each status `ErrorTypeExtensions.ToHttpStatusCode` returns, as `ApiProblemDetails` with `application/problem+json` ([ADR 0011](0011-problem-details.md)). `ApiProblemDetails` is a `ProblemDetails` that adds `traceId` and `errors`, so the document describes them. It is for the document only: nothing creates one, and a failure is sent as a plain `ProblemDetails` whose extensions carry `errors`, added by `ProblemDetailsMapper`, and `traceId`, added by the framework. An action does not declare failures. An attribute on an action replaces the base's entry for the same status, content type included.
- No controller uses `[Produces]`. It replaces an `ObjectResult`'s content types, which turns `application/problem+json` into `application/json`.
- The built-in generator serves the document at `/openapi/v1.json`, and Swagger UI serves `/swagger`. Both are mapped only in Development.
- A request DTO marks each property the caller must send `required`, which is how the document lists it as required ([ADR 0005](0005-fluent-validation.md)).
- A response DTO is typed as what a success carries. `VendorResponseDto.Id` is not nullable, because a response describes a stored vendor, while `Vendor.Id` stays null until RavenDB assigns it. `VendorMappingExtensions.ToResponseDto` throws for a vendor that has no id.
- `AddApiOpenApi` corrects what the generator gets wrong. It describes an integer or decimal as a number and not as a number or a string, names query parameters in camelCase, and removes `null` from the `data` of every `ApiResponseOf*` schema. `ApiResponse<T>.Data` is never null, but a type parameter carries no nullability, so the generator offers `null` beside the schema of `T`. That correction is a document transformer, because the generator adds the `null` after the schema transformers have run.
- `RouteOptions.LowercaseUrls` is on, set in `AddApiControllers`. Controllers keep `[Route("[controller]")]`, and the document lists their paths in lowercase, such as `/vendors`.

## Consequences

- Action signatures are longer.
- Every operation lists all six failures, including ones its use case never returns.
- Only the six `ErrorType` statuses are declared. Other statuses the API can return, such as the 502 and 504 in [ADR 0012](0012-built-in-exception-handling.md), are not.
- `ApiProblemDetails` repeats what a failure carries, which `ProblemDetailsMapper` and the framework produce. A test sends failing requests, among them bodies that leave out required properties, and checks that every member of each response is in the document.
- A success lists `text/plain` and `text/json` beside `application/json`. They are not trimmed.
- A test ties the declared statuses to `ErrorType`, so a new category or a changed mapping fails it. Another test reads the document and checks every operation.
- Routing ignores case, so a caller can use `/Vendors` or `/vendors`. `LowercaseUrls` also makes the URLs the framework generates lowercase, and nothing in the API generates one yet.
- The document does not state limits or defaults that only a validator enforces, such as the range and default of `page` and `pageSize` ([ADR 0014](0014-pagination.md)) or a positive `balance`. It describes them as plain numbers, and the validator rejects what is out of range.

## Reference implementation

- An action with its success declared, in [`VendorsController.Get`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Controllers/VendorsController.cs): `[ProducesResponseType(StatusCodes.Status200OK)]` above `public async Task<ActionResult<ApiResponse<VendorResponseDto>>> Get(string id, CancellationToken cancellationToken)`. An action that creates declares `[ProducesResponseType(StatusCodes.Status201Created)]`, as in [`VendorsController.Add`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Controllers/VendorsController.cs).
- A failure declared on the base class, in [`ApiControllerBase`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Controllers/ApiControllerBase.cs): `[ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]`
- The type the document uses for a failure, [`ApiProblemDetails`](../../src/api/PartsTrackerErp.GoldenPattern.Api/ErrorHandling/ApiProblemDetails.cs): `public required IReadOnlyList<ProblemError> Errors { get; init; }`
- A helper: [`ApiControllerBase.ToResult`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Controllers/ApiControllerBase.cs), `protected ActionResult<ApiResponse<T>> ToResult<T>(ServiceResponse<T> response)`
- Swagger UI, in [`Program.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Program.cs): `app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));`
- The test that ties the declarations to `ErrorType`: [`Given_ApiControllerBase.ApiControllerBase_DeclaredFailures_CoverTheStatusOfEveryErrorType`](../../tests/api/PartsTrackerErp.GoldenPattern.Tests/Api/Controllers/Given_ApiControllerBase.cs)
- The test of the generated document: [`Given_OpenApiDocument`](../../tests/api/PartsTrackerErp.GoldenPattern.Tests/Api/OpenApi/Given_OpenApiDocument.cs)
- The test that checks real failures against the document: [`Given_OpenApiDocument.RealFailure_EveryMember_IsDescribedByTheDocument`](../../tests/api/PartsTrackerErp.GoldenPattern.Tests/Api/OpenApi/Given_OpenApiDocument.cs)
- A response DTO built only from a stored entity: [`VendorMappingExtensions.ToResponseDto`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Mapping/VendorMappingExtensions.cs)
- The corrections to the generated document: [`OpenApiRegistration.AddApiOpenApi`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Registration/OpenApiRegistration.cs)
- Lowercase URLs: [`ControllerRegistration.AddApiControllers`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Registration/ControllerRegistration.cs)
