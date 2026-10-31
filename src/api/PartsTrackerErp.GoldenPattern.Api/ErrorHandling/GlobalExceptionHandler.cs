using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;
using PartsTrackerErp.GoldenPattern.Core.Mapping;
using PartsTrackerErp.GoldenPattern.Core.Utilities;

namespace PartsTrackerErp.GoldenPattern.Api.ErrorHandling;

/// <summary>
/// Catches exceptions thrown by later pipeline stages and answers with an <see cref="ErrorResponse"/>.
/// Register it first in <c>Program.cs</c> with <c>app.UseMiddleware&lt;GlobalExceptionHandler&gt;()</c>.
/// </summary>
public sealed class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
        {
            // The caller abandoned the request, so nobody is left to answer and nothing is wrong
            // with the service. Cancellation tokens flow through every layer, so this is routine.
            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }
        catch (Exception exception)
        {
            if (httpContext.Response.HasStarted)
            {
                throw;
            }

            if (exception is DomainException domainException)
            {
                logger.LogWarning(
                    domainException,
                    "A domain exception occurred. {Method} {Path} traceId={TraceId}",
                    httpContext.Request.Method.SanitizeForLogging(),
                    httpContext.Request.Path.Value.SanitizeForLogging(),
                    httpContext.TraceIdentifier);

                await WriteErrorResponseAsync(
                    httpContext,
                    domainException.ErrorEnvelope.ErrorType.ToHttpStatusCode(),
                    [.. domainException.Errors.Select(error => error.ToResponseItem())]);
                return;
            }

            // Every unclassified exception reaches the caller as the same "Unexpected" 500, so the
            // request context and concrete exception type are logged here or they are lost.
            InfrastructureFailure failure = exception.ClassifyInfrastructureFailure();
            logger.Log(
                failure.LogLevel,
                exception,
                "{LogMessage} {ExceptionType}: {Method} {Path} traceId={TraceId}",
                failure.LogMessage,
                exception.GetType().Name,
                httpContext.Request.Method.SanitizeForLogging(),
                httpContext.Request.Path.Value.SanitizeForLogging(),
                httpContext.TraceIdentifier);

            await WriteErrorResponseAsync(httpContext, failure.StatusCode, failure.Errors);
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext httpContext,
        int statusCode,
        IReadOnlyList<ErrorResponseItem> errors)
    {
        httpContext.Response.StatusCode = statusCode;

        var response = new ErrorResponse(httpContext.TraceIdentifier, DateTimeOffset.UtcNow, errors);
        await httpContext.Response.WriteAsJsonAsync(response, httpContext.RequestAborted);
    }
}
