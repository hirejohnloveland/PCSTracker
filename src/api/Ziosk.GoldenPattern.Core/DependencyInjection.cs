using Microsoft.Extensions.Hosting;

namespace Ziosk.GoldenPattern.Core;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Core business services. Called by every host (API, future Functions hosts).
    /// </summary>
    public static IHostApplicationBuilder AddCore(this IHostApplicationBuilder builder)
    {
        return builder;
    }
}
