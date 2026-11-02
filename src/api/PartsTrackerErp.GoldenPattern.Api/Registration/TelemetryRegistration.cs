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
            // Setting ConnectionString always overrides the distro's lookup of
            // APPLICATIONINSIGHTS_CONNECTION_STRING. Prefer that key (Azure's usual
            // App Service / Container Apps setup) so the appsettings.json placeholder
            // cannot win and send telemetry nowhere.
            string? connectionString =
                builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
                ?? builder.Configuration["ApplicationInsights:ConnectionString"];
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.ConnectionString = connectionString;
            }

            // Distro default is true: only export logs whose request trace was sampled.
            // At the default 5 traces/sec, most failure logs from ApiExceptionHandler would
            // never reach Application Insights. Keep logs regardless of trace sampling.
            options.EnableTraceBasedLogsSampler = false;
        });

        return builder;
    }
}
