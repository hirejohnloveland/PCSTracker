namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// Describes one successful response.
/// </summary>
/// <param name="TraceId">Identifies the request. Quote it when reporting a problem.</param>
/// <param name="Timestamp">When the response was created, in UTC.</param>
public sealed record ApiResponseMetadata(string TraceId, DateTime Timestamp);
