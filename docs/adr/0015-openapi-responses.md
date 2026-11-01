# ADR 0015: Actions Declare Their Responses for OpenAPI

## Context

The OpenAPI document is generated from the controllers. MVC takes an action's success body type from its declared return type, which `ActionResult<T>` can carry and `IActionResult` cannot, and declaring any response on an action or its base class switches that inference off. Every action can also fail as a problem ([ADR 0011](0011-problem-details.md)), which the generator cannot know.

## Decision

- An action in the API project returns `Task<ActionResult<TBody>>`, where `TBody` is `ApiResponse<T>` or `PagedApiResponse<T>` ([ADR 0010](0010-service-response.md)), never `IActionResult`. The protected helpers on `ApiControllerBase` return `ActionResult` or `ActionResult<TBody>`. A private helper returns its concrete type.
- An action declares its success without a type: `[ProducesResponseType(StatusCodes.Status200OK)]`, or `Status201Created` for an action that creates a resource. MVC takes the body type from `ActionResult<TBody>`. The success is always declared, because declaring any response switches off the success MVC would infer.
- `ApiControllerBase` declares the six failure responses, one for each status `ErrorTypeExtensions.ToHttpStatusCode` returns, as `ProblemDetails` with `application/problem+json` ([ADR 0011](0011-problem-details.md)). An action does not declare failures. An attribute on an action replaces the base's entry for the same status, content type included.
- No controller uses `[Produces]`. It replaces an `ObjectResult`'s content types, which turns `application/problem+json` into `application/json`.
- The built-in generator serves the document at `/openapi/v1.json`, and Swagger UI serves `/swagger`. Both are mapped only in Development.

## Consequences

- Action signatures are longer.
- Every operation lists all six failures, including ones its use case never returns.
- Only the six `ErrorType` statuses are declared. Other statuses the API can return, such as the 502 and 504 in [ADR 0012](0012-built-in-exception-handling.md), are not.
- A failure is documented as the framework's `ProblemDetails`, so the document does not list `errors` or `traceId`.
- A success lists `text/plain` and `text/json` beside `application/json`. They are not trimmed.
- A test ties the declared statuses to `ErrorType`, so a new category or a changed mapping fails it. Another test reads the document and checks every operation.

## Reference implementation

- An action with its success declared, in [`VendorsController.Get`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Controllers/VendorsController.cs): `[ProducesResponseType(StatusCodes.Status200OK)]` above `public async Task<ActionResult<ApiResponse<VendorResponseDto>>> Get(string id, CancellationToken cancellationToken)`. An action that creates declares `[ProducesResponseType(StatusCodes.Status201Created)]`, as in [`VendorsController.Add`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Controllers/VendorsController.cs).
- A failure declared on the base class, in [`ApiControllerBase`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Controllers/ApiControllerBase.cs): `[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]`
- A helper: [`ApiControllerBase.ToResult`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Controllers/ApiControllerBase.cs), `protected ActionResult<ApiResponse<T>> ToResult<T>(ServiceResponse<T> response)`
- Swagger UI, in [`Program.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Program.cs): `app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));`
- The test that ties the declarations to `ErrorType`: [`Given_ApiControllerBase.ApiControllerBase_DeclaredFailures_CoverTheStatusOfEveryErrorType`](../../tests/api/PartsTrackerErp.GoldenPattern.Tests/Api/Controllers/Given_ApiControllerBase.cs)
- The test of the generated document: [`Given_OpenApiDocument`](../../tests/api/PartsTrackerErp.GoldenPattern.Tests/Api/OpenApi/Given_OpenApiDocument.cs)
