using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Api.ErrorHandling;

namespace PartsTrackerErp.GoldenPattern.Api.Registration;

internal static class ErrorHandlingRegistration
{
    internal static IHostApplicationBuilder AddApiErrorHandling(this IHostApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemDetailsMapper.Customize);
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();

        // [ApiController] answers an unbindable request with a field-keyed errors object that would clash with ours.
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = context =>
                ProblemDetailsMapper.ToProblemDetails(context.ModelState).ToObjectResult(context.HttpContext));

        return builder;
    }
}
