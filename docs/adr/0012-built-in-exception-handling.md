# ADR 0012: Unexpected Exceptions Use .NET's Built-In Exception Handling

## Context

An unexpected exception has to reach the caller as a safe response, and its details have to reach the log. ASP.NET Core has built-in exception handling that writes problem details, and it accepts `IExceptionHandler` implementations for exceptions an application wants to answer itself.

## Decision

- `Program.cs` calls `UseExceptionHandler()` and `UseStatusCodePages()` first, so they wrap every later stage ([ADR 0001](0001-host-registration-pattern.md)). `AddApiErrorHandling()` (`Registration/ErrorHandlingRegistration.cs`), called from `AddApi()`, registers `AddProblemDetails` and `ApiExceptionHandler`. There is no custom exception middleware.
- `ApiExceptionHandler` answers four cases:
  - `DomainException`, by its category, through `ProblemDetailsMapper` ([ADR 0011](0011-problem-details.md)).
  - `BadHttpRequestException`, with its own status. .NET 10 would otherwise answer these 500.
  - `HttpRequestException`, as 502 with code `UpstreamServiceUnavailable`.
  - An `HttpClient` timeout, as 504 with code `UpstreamServiceTimeout`.
- Any other exception makes the handler return false. The framework logs it at Error and writes a 500 problem with no stack trace or exception text. The framework also handles a caller that disconnects.
- .NET 10 does not log an exception that an `IExceptionHandler` handles, so `ApiExceptionHandler` logs it: Error for a `DomainException` that maps to a 5xx status, Warning for the rest. Method and path are sanitized with `SanitizeForLogging`.
- The handler sets the status and returns true even if the problem writer declines because the client's `Accept` header excludes JSON. Returning false would let the middleware's 404 guard turn a handled 404 into a 500.
- Request validation and failures a service returns do not use exceptions ([ADR 0010](0010-service-response.md)).

## Consequences

- An exception type gets its own response only by adding a case to `ApiExceptionHandler`. Until then it is a 500.
- A caller sees a generic message for an unexpected failure. The cause is in the log.
