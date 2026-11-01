namespace PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

/// <summary>
/// One page of entities, and the number of entities across every page.
/// </summary>
/// <param name="Items">The entities on the page.</param>
/// <param name="TotalCount">The number of entities that match, across every page.</param>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);
