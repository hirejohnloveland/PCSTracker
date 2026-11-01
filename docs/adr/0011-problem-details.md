# ADR 0011: Failures Reach Callers as RFC 9457 Problem Details

## Context

An API client has to handle failures from domain rules, request validation, unexpected exceptions, and the framework itself, such as routing and authentication. RFC 9457 defines a JSON problem format for HTTP APIs. It allows extension members and tells clients to ignore members they do not know.

## Decision

- Every failure is `application/problem+json`: the RFC's members, plus an `errors` array.
- Each `errors` entry is a `ProblemError` ([`Contracts/Responses/ProblemError.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Contracts/Responses/ProblemError.cs)) with `code`, `detail`, and `pointer`. `pointer` is a JSON Pointer into the request body, such as `#/name`, or `#/` and the name of a query parameter, such as `#/pageSize`. It appears only for request problems. The array follows the example in RFC 9457 and adds `code`.
- A response has one category, the `ErrorType` ([ADR 0010](0010-service-response.md)), and it sets `status` and `type`. RFC 9457 recommends reporting only the most relevant problem when problems differ in type. `ErrorTypeExtensions.ToHttpStatusCode` is the only mapping: Validation 400, Unauthorized 401, Forbidden 403, NotFound 404, BusinessRuleViolation 409, Unexpected 500, and anything else 500.
- `type` is `https://api.example.com/problems/{category}` with the category in kebab-case, such as `business-rule-violation`. The base is one constant in `ProblemDetailsMapper`. A status with no category keeps the framework's `type`.
- `title` is "Validation Failed" for validation and the framework's title for the status otherwise. `detail` is the message of a single error, or a generic summary when there are several.
- Exception text never reaches the caller.
- A request that fails FluentValidation is a 400 with one entry per failure. `code` is the failure's error code and `pointer` is the camelCased property path. A request MVC cannot bind has the same shape with code `InvalidRequest`, through `ApiBehaviorOptions.InvalidModelStateResponseFactory`.
- `ProblemDetailsMapper.Customize` is registered with `AddProblemDetails`. It sets `instance`, sets `type` for a status that has a category, and adds one default `errors` entry when a problem has none. Problems the framework builds, such as a routing 404 or an authentication 401, then have the same shape. `UseStatusCodePages` gives those responses a body.
- `traceId` is the W3C trace id when there is one, otherwise `HttpContext.TraceIdentifier`. A success carries the same value in `metadata.traceId`.

## Consequences

- A client reads `status` and `type` for the category and `errors[].code` for the failure.
- `errors` is an array, not ASP.NET's default field-keyed object, so each error carries its own code.
- Nothing serves documents at the `type` URIs, and the base host is `api.example.com` for now.

## Reference implementation

- Type: [`ProblemError.cs`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Contracts/Responses/ProblemError.cs), constructed as `new ProblemError("UpstreamServiceUnavailable", "A required upstream service is unavailable.", null)`.
- Building the problem for a domain failure: [`ProblemDetailsMapper.ToProblemDetails(DomainErrorEnvelope)`](../../src/api/PartsTrackerErp.GoldenPattern.Api/ErrorHandling/ProblemDetailsMapper.cs)
- Giving a framework-built problem the same shape: [`ProblemDetailsMapper.Customize`](../../src/api/PartsTrackerErp.GoldenPattern.Api/ErrorHandling/ProblemDetailsMapper.cs)
- The status mapping: [`ErrorTypeExtensions.ToHttpStatusCode`](../../src/api/PartsTrackerErp.GoldenPattern.Api/ErrorHandling/ErrorTypeExtensions.cs)
