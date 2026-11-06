# ADR 0013: Observability with OpenTelemetry and Azure Monitor

## Context

A failure or a slow request has to be diagnosable from what the running service reports, not from reproducing it locally. The .NET OpenTelemetry SDK is the standard instrumentation API for traces, metrics, and logs. Azure Monitor (Application Insights) is this template's destination for them. Microsoft publishes `Azure.Monitor.OpenTelemetry.AspNetCore` (the "Azure Monitor Distro") as the supported way to connect the two: one call wires tracing, metrics, and logging together, with ASP.NET Core and HttpClient instrumentation included.

This ADR is more detailed than the others in this set on purpose: observability conventions are easy to drift on method by method.

## Decision

### Wiring

- `Registration/TelemetryRegistration.cs`'s `AddApiTelemetry()`, called from `AddApi()` ([ADR 0001](0001-host-registration-pattern.md)), registers the distro with `AddOpenTelemetry().UseAzureMonitor(...)`.
- This lives in the API host's `Registration/`, not in Core. The distro's automatic instrumentation hooks the ASP.NET Core request pipeline, which makes it a host concern the same way authentication and the exception handler are ([ADR 0001](0001-host-registration-pattern.md)) — not something every future host would wire up identically.
- The connection string prefers `APPLICATIONINSIGHTS_CONNECTION_STRING` (Azure's usual App Service / Container Apps setup), then falls back to `ApplicationInsights:ConnectionString`. `appsettings.json` holds a placeholder value; each environment supplies its own real one. The registration only assigns `options.ConnectionString` when a non-empty value is found, so setting the property cannot override the distro's own env lookup with the placeholder.
- No separate logging provider is registered. `UseAzureMonitor()` adds an `ILoggerProvider` to DI, and `LoggerFactory` resolves every registered provider automatically. Every logger already in the codebase — `ApiExceptionHandler` via `ILogger<T>`, `ApiControllerBase` via `ILoggerFactory` — is captured without changes to any of them.
- `EnableTraceBasedLogsSampler` is set to `false`. The distro default is `true`: a log with a request `SpanId` is exported only when that request's trace was sampled. Combined with the default 5 traces/sec rate limit, most `ApiExceptionHandler` Warning/Error logs would never reach Application Insights under load. Disabling keeps logs regardless of whether the request's trace was kept; a busy endpoint can still have gaps in its *trace* data.

### Structured logging

- Write a message template with named placeholders. Never interpolate values into the message string: a placeholder becomes a queryable field in Application Insights, interpolated text is just a flattened string.
- Pass an `Exception` as the dedicated `exception` parameter, never formatted into the message text. Application Insights then records `ExceptionTelemetry` with a stack trace instead of a plain `TraceTelemetry`.
- Sanitize any string that came from the caller — request path, header, query value, user input — with `SanitizeForLogging()` before it reaches a log call, so it can't forge extra log lines.
- Choose the level deliberately, not by habit: `Error` for a failure that is a bug or reaches the caller as a 5xx, `Warning` for an expected failure logged for visibility (validation-shaped, upstream unavailable), `Information` for a normal lifecycle event.
- A log's `traceId` is `Activity.Current?.Id` when there is a current activity — the W3C traceparent form `00-{trace-id}-{span-id}-{flags}`, not the bare 32-hex W3C trace id — falling back to `HttpContext.TraceIdentifier` otherwise. The same value is returned to callers ([ADR 0011](0011-problem-details.md)). ASP.NET Core starts that activity for the request when a listener is present; the distro is such a listener. Application Insights keys a transaction by `operation_Id`, which is only the `{trace-id}` segment, so paste that middle part — not the whole `traceId` — into transaction search.

### Custom metrics

- No application-defined metric exists yet. `System.Diagnostics.Metrics` (`Meter`, `Counter<T>`, `Histogram<T>`) is already the API OpenTelemetry reads, is already part of the .NET runtime, and needs no wrapper or interface — the same way `ILogger<T>` has none.
- Unlike `ILogger<T>`, a `Meter` is invisible to the exporter until its name is registered. The built-in ASP.NET Core and HttpClient metrics are registered by the distro already; an application-defined one is not. Add this to `AddApiTelemetry()` when the first custom metric is written, and give every application `Meter` the same name (or a name under the same prefix), so one registration covers all of them:

```csharp
builder.Services.ConfigureOpenTelemetryMeterProvider(
    (_, metrics) => metrics.AddMeter("PartsTrackerErp.GoldenPattern"));
```

## Consequences

- The distro rate-limits trace sampling to 5 per second by default. A busy endpoint will have gaps in trace data unless `AzureMonitorOptions.TracesPerSecond` or `SamplingRatio` is set explicitly. Trace sampling alone no longer drops the matching logs — see `EnableTraceBasedLogsSampler = false` above — but a log from an unsampled request will not have a matching request transaction to open in Application Insights; search by the log's `traceId` / `operation_Id` instead.
- Live Metrics (real-time monitoring) is on by default and holds a low-overhead background connection open; `options.EnableLiveMetrics = false` turns it off if that's ever unwanted.
- Until a real connection string replaces the placeholder, the exporters run and retry against a destination that doesn't exist. This does not fail startup or any request — it is silent background noise until an environment supplies its real value.
- `ILogger.BeginScope` data is dropped, not exported, unless `OpenTelemetryLoggerOptions.IncludeScopes` is set to `true`. This template does not enable it, so a future `BeginScope` call would silently produce no extra data in Application Insights.
- A custom metric that is added without also adding its `AddMeter` registration will run with no error and produce no data in Azure Monitor.

## Reference implementation

- Registration: [`TelemetryRegistration.AddApiTelemetry`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Registration/TelemetryRegistration.cs)
- Structured log with an `Exception` argument and sanitized method/path, in [`ApiExceptionHandler.TryHandleAsync`](../../src/api/PartsTrackerErp.GoldenPattern.Api/ErrorHandling/ApiExceptionHandler.cs): `logger.Log(..., exception, "{ExceptionType} answered with {Status}: {Method} {Path} traceId={TraceId}", ...)`
- A 5xx use-case failure logged through `ILoggerFactory`, in [`ApiControllerBase.ToFailureResult`](../../src/api/PartsTrackerErp.GoldenPattern.Api/Controllers/ApiControllerBase.cs): `GetRequiredService<ILoggerFactory>().CreateLogger(GetType()).LogError(...)`
- Sanitizing caller-supplied text: [`LogSanitizationExtensions.SanitizeForLogging`](../../src/api/PartsTrackerErp.GoldenPattern.Core/Utilities/LogSanitizationExtensions.cs)
- The shared `traceId`: [`ProblemDetailsMapper.GetTraceId`](../../src/api/PartsTrackerErp.GoldenPattern.Api/ErrorHandling/ProblemDetailsMapper.cs)
