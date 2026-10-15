namespace Ziosk.GoldenPattern.Api.Registration;

internal static class ControllerRegistration
{
    internal static IHostApplicationBuilder AddApiControllers(this IHostApplicationBuilder builder)
    {
        builder.Services.AddControllers();

        return builder;
    }
}
