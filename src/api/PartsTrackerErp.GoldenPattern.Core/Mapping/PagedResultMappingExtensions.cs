using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Core.Mapping;

public static class PagedResultMappingExtensions
{
    /// <summary>
    /// Maps one page of entities to a page of response DTOs and works out how many pages there are.
    /// <paramref name="page"/> and <paramref name="pageSize"/> are the ones the page was fetched with.
    /// </summary>
    public static PagedResponseDto<TDto> ToPagedResponseDto<TEntity, TDto>(
        this PagedResult<TEntity> result,
        int page,
        int pageSize,
        Func<TEntity, TDto> map)
    {
        int totalPages = (result.TotalCount / pageSize) + (result.TotalCount % pageSize == 0 ? 0 : 1);

        return new PagedResponseDto<TDto>(
            Items: [.. result.Items.Select(map)],
            Pagination: new PaginationMetadata(
                Page: page,
                PageSize: pageSize,
                TotalCount: result.TotalCount,
                TotalPages: totalPages));
    }
}
