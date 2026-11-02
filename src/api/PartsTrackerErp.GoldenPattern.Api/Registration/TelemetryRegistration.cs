using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace PartsTrackerErp.GoldenPattern.Api.Registration;

internal static class TelemetryRegistration
{
    /// <summary>
    /// Sends traces, metrics, and logs to Application Insights. Every <see cref="ILogger{TCategoryName}"/>
    /// is captured automatically; no separate logging provider is needed.
    /// </summary>
    internal static IHostApplicationBuilder AddApiTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
        {
            options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
            // Distro default is true: only export logs whose request trace was sampled.
            // At the default 5 traces/sec, most failure logs from ApiExceptionHandler would
            // never reach Application Insights. Keep logs regardless of trace sampling.
            options.EnableTraceBasedLogsSampler = false;
        });

        return builder;
    }
}
