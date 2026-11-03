namespace PartsTrackerErp.GoldenPattern.Api.Registration;

internal static class ControllerRegistration
{
    internal static IHostApplicationBuilder AddApiControllers(this IHostApplicationBuilder builder)
    {
        builder.Services.AddControllers();

        // Controllers keep [Route("[controller]")]. This lowercases the URLs the framework generates and the paths in the
        // OpenAPI document. Routing ignores case, so it does not change which requests match.
        builder.Services.AddRouting(options => options.LowercaseUrls = true);

        return builder;
    }
}
