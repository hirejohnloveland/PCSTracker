namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// One page of response DTOs, as an application service returns it. The API sends it as a
/// <see cref="PagedApiResponse{T}"/>.
/// </summary>
/// <param name="Items">The items on the page.</param>
/// <param name="Pagination">Where the page sits in the whole list.</param>
public sealed record PagedResponseDto<T>(IReadOnlyList<T> Items, PaginationMetadata Pagination);
