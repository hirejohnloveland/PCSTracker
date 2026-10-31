namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// The body of every error response from the API.
/// </summary>
/// <param name="TraceId">Identifies the request. Quote it when reporting a problem.</param>
/// <param name="Timestamp">When the response was created, in UTC.</param>
/// <param name="Errors">What went wrong, one item per failure.</param>
public sealed record ErrorResponse(
    string TraceId,
    DateTimeOffset Timestamp,
    IReadOnlyList<ErrorResponseItem> Errors);
