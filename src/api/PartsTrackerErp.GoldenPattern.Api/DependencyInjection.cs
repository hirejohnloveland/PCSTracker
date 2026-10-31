using PartsTrackerErp.GoldenPattern.Api.Registration;

namespace PartsTrackerErp.GoldenPattern.Api;

public static class DependencyInjection
{
    /// <summary>
    /// Registers API host services.
    /// </summary>
    public static IHostApplicationBuilder AddApi(this IHostApplicationBuilder builder)
    {
        builder.AddApiAuthentication();
        builder.AddApiControllers();
        builder.AddApiOpenApi();

        return builder;
    }
}
