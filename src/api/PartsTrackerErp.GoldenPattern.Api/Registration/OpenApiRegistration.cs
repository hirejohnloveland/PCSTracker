namespace PartsTrackerErp.GoldenPattern.Api.Registration;

internal static class OpenApiRegistration
{
    internal static IHostApplicationBuilder AddApiOpenApi(this IHostApplicationBuilder builder)
    {
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        return builder;
    }
}
