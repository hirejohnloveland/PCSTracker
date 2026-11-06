using System.Text.Json.Serialization.Metadata;
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
        builder.Services.Configure<ApiBehaviorOptions>(
            options => options.InvalidModelStateResponseFactory = CreateInvalidModelStateResponse);

        // MVC puts System.Text.Json's error text in the model state unless this is off. Off, the error has no message
        // and CreateInvalidModelStateResponse gives it a generic one.
        builder.Services.Configure<JsonOptions>(options => options.AllowInputFormatterExceptionMessages = false);

        // A request that leaves out a property binds with it unset, and the request's validator reports it by name at
        // its pointer. Left to MVC, a missing property fails the binding, and the error names only the whole body.
        builder.Services.Configure<JsonOptions>(options =>
            options.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { LeavePresenceToValidators }
            });
        builder.Services.Configure<MvcOptions>(
            options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

        return builder;
    }

    private static void LeavePresenceToValidators(JsonTypeInfo typeInfo)
    {
        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            property.IsRequired = false;
        }
    }

    private static ObjectResult CreateInvalidModelStateResponse(ActionContext context)
    {
        return ProblemDetailsMapper.ToProblemDetails(context.ModelState).ToObjectResult(context.HttpContext);
    }
}
