namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// The body of every successful response that carries one page of a list.
/// </summary>
/// <param name="Data">The items on the page.</param>
/// <param name="Pagination">Where the page sits in the whole list.</param>
/// <param name="Metadata">Describes the response.</param>
public sealed record PagedApiResponse<T>(IReadOnlyList<T> Data, PaginationMetadata Pagination, ApiResponseMetadata Metadata);
