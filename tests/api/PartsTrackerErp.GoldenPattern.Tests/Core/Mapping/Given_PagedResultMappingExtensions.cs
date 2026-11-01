using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Mapping;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.Mapping;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_PagedResultMappingExtensions
{
    [Fact]
    public void ToPagedResponseDto_MapsEachItemInOrder()
    {
        var result = new PagedResult<string>(["b", "a", "c"], 3);

        PagedResponseDto<string> response = result.ToPagedResponseDto(1, 25, item => item.ToUpperInvariant());

        Assert.Equal(["B", "A", "C"], response.Items);
    }

    [Fact]
    public void ToPagedResponseDto_CopiesThePageAndPageSizeAndTheTotalCount()
    {
        var result = new PagedResult<string>(["k"], 21);

        PagedResponseDto<string> response = result.ToPagedResponseDto(3, 10, item => item);

        Assert.Equal(new PaginationMetadata(Page: 3, PageSize: 10, TotalCount: 21, TotalPages: 3), response.Pagination);
    }

    [Fact]
    public void ToPagedResponseDto_PagePastTheEnd_HasNoItemsButKeepsTheTotals()
    {
        var result = new PagedResult<string>([], 21);

        PagedResponseDto<string> response = result.ToPagedResponseDto(9, 10, item => item);

        Assert.Empty(response.Items);
        Assert.Equal(new PaginationMetadata(Page: 9, PageSize: 10, TotalCount: 21, TotalPages: 3), response.Pagination);
    }

    [Theory]
    [InlineData(0, 25, 0)]
    [InlineData(1, 25, 1)]
    [InlineData(25, 25, 1)]
    [InlineData(26, 25, 2)]
    [InlineData(50, 25, 2)]
    [InlineData(51, 25, 3)]
    [InlineData(10, 1, 10)]
    [InlineData(int.MaxValue, 100, 21_474_837)]
    public void ToPagedResponseDto_CountsThePagesNeededForTheTotal(int totalCount, int pageSize, int expectedTotalPages)
    {
        var result = new PagedResult<string>([], totalCount);

        PagedResponseDto<string> response = result.ToPagedResponseDto(1, pageSize, item => item);

        Assert.Equal(expectedTotalPages, response.Pagination.TotalPages);
    }
}
