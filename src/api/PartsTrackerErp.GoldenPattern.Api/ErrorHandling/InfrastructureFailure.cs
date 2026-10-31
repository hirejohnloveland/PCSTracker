using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Api.ErrorHandling;

/// <summary>
/// How to log and answer an exception that is not a <c>DomainException</c>.
/// </summary>
public sealed record InfrastructureFailure(
    LogLevel LogLevel,
    string LogMessage,
    int StatusCode,
    IReadOnlyList<ErrorResponseItem> Errors);
