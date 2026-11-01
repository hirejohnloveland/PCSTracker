namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// Where a page sits in the whole list.
/// </summary>
/// <param name="Page">The 1-based number of the page that was requested.</param>
/// <param name="PageSize">The number of items requested per page.</param>
/// <param name="TotalCount">The number of items across every page.</param>
/// <param name="TotalPages">The number of pages. 0 when there are no items.</param>
public sealed record PaginationMetadata(int Page, int PageSize, int TotalCount, int TotalPages);
