using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;
using PartsTrackerErp.GoldenPattern.Core.Utilities;

namespace PartsTrackerErp.GoldenPattern.Api.ErrorHandling;

/// <summary>
/// Answers the exceptions the API expects with a problem response. Any other exception returns false, and
/// the framework logs it at Error and writes a safe 500.
/// </summary>
public sealed class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails? problem = exception switch
        {
            DomainException domainException => ProblemDetailsMapper.ToProblemDetails(domainException.ErrorEnvelope),

            // .NET would answer these 500. They already carry the status that describes them.
            BadHttpRequestException badRequest => ProblemDetailsMapper.ToProblemDetails(badRequest.StatusCode),

            // A non-success response, or no connection at all.
            HttpRequestException => ProblemDetailsMapper.ToProblemDetails(
                StatusCodes.Status502BadGateway,
                new ProblemError("UpstreamServiceUnavailable", "A required upstream service is unavailable.", null)),

            // Not caller cancellation: HttpClient sets this inner exception when its own Timeout elapses.
            TaskCanceledException { InnerException: TimeoutException } => ProblemDetailsMapper.ToProblemDetails(
                StatusCodes.Status504GatewayTimeout,
                new ProblemError("UpstreamServiceTimeout", "A required upstream service did not respond in time.", null)),

            _ => null
        };

        if (problem is null)
        {
            return false;
        }

        int status = problem.Status!.Value;

        // .NET does not log an exception that an IExceptionHandler handles.
        logger.Log(
            exception is DomainException && status >= StatusCodes.Status500InternalServerError
                ? LogLevel.Error
                : LogLevel.Warning,
            exception,
            "{ExceptionType} answered with {Status}: {Method} {Path} traceId={TraceId}",
            exception.GetType().Name,
            status,
            httpContext.Request.Method.SanitizeForLogging(),
            httpContext.Request.Path.Value.SanitizeForLogging(),
            ProblemDetailsMapper.GetTraceId(httpContext));

        httpContext.Response.StatusCode = status;

        // The writer declines when the client's Accept header excludes JSON. The status still stands, and
        // returning false would let the middleware's 404 guard turn a handled 404 into a 500.
        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            AdditionalMetadata = httpContext.Features.Get<IExceptionHandlerFeature>()?.Endpoint?.Metadata,
            ProblemDetails = problem,
            Exception = exception
        });

        return true;
    }
}
