# ADR 0013: Observability with OpenTelemetry and Azure Monitor

## Context

A failure or a slow request has to be diagnosable from what the running service reports, not from reproducing it locally. The .NET OpenTelemetry SDK is the standard instrumentation API for traces, metrics, and logs. Azure Monitor (Application Insights) is this template's destination for them. Microsoft publishes `Azure.Monitor.OpenTelemetry.AspNetCore` (the "Azure Monitor Distro") as the supported way to connect the two: one call wires tracing, metrics, and logging together, with ASP.NET Core and HttpClient instrumentation included.

This ADR is more detailed than the others in this set on purpose: observability conventions are easy to drift on method by method, so the examples here are meant to be copied, not just summarized.

## Decision

### Wiring

- `Registration/TelemetryRegistration.cs`'s `AddApiTelemetry()`, called from `AddApi()` ([ADR 0001](0001-host-registration-pattern.md)), registers the distro:

```csharp
builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableTraceBasedLogsSampler = false;
});
```

- This lives in the API host's `Registration/`, not in Core. The distro's automatic instrumentation hooks the ASP.NET Core request pipeline, which makes it a host concern the same way authentication and the exception handler are ([ADR 0001](0001-host-registration-pattern.md)) — not something every future host would wire up identically.
- The connection string is read from the `ApplicationInsights:ConnectionString` configuration key, the same way `RavenDb` and `AzureAd` each have their own section ([ADR 0006](0006-ravendb-persistence.md)). `appsettings.json` holds a placeholder value; each environment supplies its own real one.
- No separate logging provider is registered. `UseAzureMonitor()` adds an `ILoggerProvider` to DI, and `LoggerFactory` resolves every registered provider automatically. Every `ILogger<T>` already in the codebase — `ApiExceptionHandler`, `RavenDbDatabaseInitializer`, `ApiControllerBase` — is captured without changes to any of them.
- `EnableTraceBasedLogsSampler` is set to `false`. The distro default is `true`: a log with a request `SpanId` is exported only when that request's trace was sampled. Combined with the default 5 traces/sec rate limit, most `ApiExceptionHandler` Warning/Error logs would never reach Application Insights under load. Disabling keeps logs regardless of whether the request's trace was kept; a busy endpoint can still have gaps in its *trace* data.

### Structured logging

- Write a message template with named placeholders. Never interpolate values into the message string: a placeholder becomes a queryable field in Application Insights, interpolated text is just a flattened string.

```csharp
logger.Log(
    exception is DomainException && status >= StatusCodes.Status500InternalServerError
        ? LogLevel.Error
        : LogLevel.Warning,
    exception,
    "{ExceptionType} answered with {Status}: {Method} {Path} traceId={TraceId}",
    exception.GetType().Name,
    status,
    httpContext.Request.Method.SanitizeForLogging(),
    httpContext.Request.Path.Value.SanitizeForLogging(),
    ProblemDetailsMapper.GetTraceId(httpContext));
```

(`ApiExceptionHandler.TryHandleAsync`, condensed)

- Pass an `Exception` as the dedicated `exception` parameter, never formatted into the message text. Application Insights then records `ExceptionTelemetry` with a stack trace instead of a plain `TraceTelemetry`.
- Sanitize any string that came from the caller — request path, header, query value, user input — with `SanitizeForLogging()` (`Core/Utilities/LogSanitizationExtensions.cs`) before it reaches a log call, so it can't forge extra log lines.
- Choose the level deliberately, not by habit: `Error` for a failure that is a bug or reaches the caller as a 5xx, `Warning` for an expected failure logged for visibility (validation-shaped, upstream unavailable), `Information` for a normal lifecycle event such as `RavenDbDatabaseInitializer` creating the database once.
- A log's `traceId` is `Activity.Current?.Id` when there is a current activity — the W3C traceparent form `00-{trace-id}-{span-id}-{flags}`, not the bare 32-hex W3C trace id — falling back to `HttpContext.TraceIdentifier` otherwise:

```csharp
public static string GetTraceId(HttpContext httpContext)
{
    return Activity.Current?.Id ?? httpContext.TraceIdentifier;
}
```

(`ProblemDetailsMapper.GetTraceId`, also used for the `traceId` returned to callers — [ADR 0011](0011-problem-details.md)). ASP.NET Core starts that activity for the request when a listener is present; the distro is such a listener. Application Insights keys a transaction by `operation_Id`, which is only the `{trace-id}` segment, so paste that middle part — not the whole `traceId` — into transaction search.

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
