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
            options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]);

        return builder;
    }
}
