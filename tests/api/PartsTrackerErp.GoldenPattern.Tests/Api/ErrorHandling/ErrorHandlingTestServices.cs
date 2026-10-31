using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PartsTrackerErp.GoldenPattern.Api.ErrorHandling;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.ErrorHandling;

internal static class ErrorHandlingTestServices
{
    /// <summary>
    /// The framework services the error handling relies on, registered as <c>ErrorHandlingRegistration</c> does.
    /// </summary>
    public static ServiceProvider Build(CapturingLoggerProvider? logs = null)
    {
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            if (logs is not null)
            {
                builder.AddProvider(logs);
            }
        });
        services.AddControllers();
        services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemDetailsMapper.Customize);

        return services.BuildServiceProvider();
    }

    public static DefaultHttpContext NewHttpContext(IServiceProvider services, string path = "/vendors")
    {
        return new()
        {
            RequestServices = services,
            Request = { Method = "POST", Path = path },
            Response = { Body = new MemoryStream() }
        };
    }

    public static string ReadBody(HttpContext httpContext)
    {
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        return new StreamReader(httpContext.Response.Body).ReadToEnd();
    }
}
