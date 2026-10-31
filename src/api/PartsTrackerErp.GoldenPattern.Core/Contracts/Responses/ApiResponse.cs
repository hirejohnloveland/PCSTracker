namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// The body of every successful response from the API.
/// </summary>
/// <param name="Data">The result of the use case.</param>
/// <param name="Metadata">Describes the response.</param>
public sealed record ApiResponse<T>(T Data, ApiResponseMetadata Metadata);
