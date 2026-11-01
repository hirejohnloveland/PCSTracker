namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// The body of a successful response that carries one result. A response that carries a page of a list is a
/// <see cref="PagedApiResponse{T}"/>.
/// </summary>
/// <param name="Data">The result of the use case.</param>
/// <param name="Metadata">Describes the response.</param>
public sealed record ApiResponse<T>(T Data, ApiResponseMetadata Metadata);
