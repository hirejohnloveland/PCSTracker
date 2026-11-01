namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

/// <summary>
/// The paging part of a list request. A list request derives from it and adds its own filters.
/// </summary>
public abstract class PagedRequestDto
{
    public const int DefaultPageSize = 25;

    public const int MaxPageSize = 100;

    /// <summary>
    /// The 1-based number of the page to return.
    /// </summary>
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = DefaultPageSize;
}
