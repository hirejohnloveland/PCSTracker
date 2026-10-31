using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Api.ErrorHandling;

public static class InfrastructureExceptionExtensions
{
    /// <summary>
    /// The non-domain counterpart to <see cref="ErrorTypeExtensions.ToHttpStatusCode"/>: classifies an
    /// exception into a log level, status code, and error body.
    /// </summary>
    public static InfrastructureFailure ClassifyInfrastructureFailure(this Exception exception) => exception switch
    {
        // A non-success response, or no connection at all.
        HttpRequestException => new InfrastructureFailure(
            LogLevel.Warning,
            "An outbound call to a dependency failed.",
            StatusCodes.Status502BadGateway,
            [new ErrorResponseItem("UpstreamServiceUnavailable", "A required upstream service is unavailable.")]),

        // Not caller cancellation: HttpClient sets this inner exception when its own Timeout elapses.
        TaskCanceledException { InnerException: TimeoutException } => new InfrastructureFailure(
            LogLevel.Warning,
            "An outbound call to a dependency timed out.",
            StatusCodes.Status504GatewayTimeout,
            [new ErrorResponseItem("UpstreamServiceTimeout", "A required upstream service did not respond in time.")]),

        _ => new InfrastructureFailure(
            LogLevel.Error,
            "An unexpected exception occurred.",
            StatusCodes.Status500InternalServerError,
            [new ErrorResponseItem("Unexpected", "An unexpected error occurred.")])
    };
}
