using FluentValidation.Results;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Validation.Validators;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.Validation.Validators;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_ListRefrigerantLogEntriesDtoValidator
{
    private readonly ListRefrigerantLogEntriesDtoValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_NoFilters_IsValid()
    {
        ValidationResult result = await _validator.ValidateAsync(new ListRefrigerantLogEntriesDto());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_SameFromAndToDate_IsValid()
    {
        var date = new DateOnly(2026, 5, 15);

        ValidationResult result = await _validator.ValidateAsync(
            new ListRefrigerantLogEntriesDto { DateFrom = date, DateTo = date });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_ToDateBeforeFromDate_FailsOnDateTo()
    {
        ValidationResult result = await _validator.ValidateAsync(
            new ListRefrigerantLogEntriesDto { DateFrom = new DateOnly(2026, 5, 15), DateTo = new DateOnly(2026, 5, 14) });

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("DateTo", failure.PropertyName);
        Assert.Equal("Date to must be on or after date from.", failure.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_PageSizeOverTheMaximum_Fails()
    {
        ValidationResult result = await _validator.ValidateAsync(
            new ListRefrigerantLogEntriesDto { PageSize = PagedRequestDto.MaxPageSize + 1 });

        Assert.False(result.IsValid);
    }
}
