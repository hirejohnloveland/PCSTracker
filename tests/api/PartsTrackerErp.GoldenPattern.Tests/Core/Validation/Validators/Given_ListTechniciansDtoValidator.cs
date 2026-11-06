using FluentValidation.Results;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Validation.Validators;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.Validation.Validators;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_ListTechniciansDtoValidator
{
    private readonly ListTechniciansDtoValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_Defaults_AreValidAndAskForTheFirstPageOf25()
    {
        var request = new ListTechniciansDto();

        ValidationResult result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
        Assert.Equal(1, request.Page);
        Assert.Equal(25, request.PageSize);
        Assert.Null(request.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task ValidateAsync_PageBelowOne_FailsOnPage(int page)
    {
        ValidationResult result = await _validator.ValidateAsync(new ListTechniciansDto { Page = page });

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("Page", failure.PropertyName);
        Assert.Equal("Page must be 1 or greater.", failure.ErrorMessage);
        Assert.Equal("GreaterThanOrEqualValidator", failure.ErrorCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task ValidateAsync_PageSizeAtEitherLimit_IsValid(int pageSize)
    {
        ValidationResult result = await _validator.ValidateAsync(new ListTechniciansDto { PageSize = pageSize });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(int.MaxValue)]
    public async Task ValidateAsync_PageSizeOutsideTheLimits_FailsOnPageSize(int pageSize)
    {
        ValidationResult result = await _validator.ValidateAsync(new ListTechniciansDto { PageSize = pageSize });

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("PageSize", failure.PropertyName);
        Assert.Equal("Page size must be between 1 and 100.", failure.ErrorMessage);
        Assert.Equal("InclusiveBetweenValidator", failure.ErrorCode);
    }

    [Theory]
    [InlineData(21_474_837, 100)]
    [InlineData(int.MaxValue, 1)]
    public async Task ValidateAsync_LastPageWhoseOffsetFitsAnInt_IsValid(int page, int pageSize)
    {
        ValidationResult result = await _validator.ValidateAsync(new ListTechniciansDto { Page = page, PageSize = pageSize });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(21_474_838, 100)]
    [InlineData(int.MaxValue, 2)]
    public async Task ValidateAsync_PageWhoseOffsetOverflowsAnInt_FailsOnPage(int page, int pageSize)
    {
        ValidationResult result = await _validator.ValidateAsync(new ListTechniciansDto { Page = page, PageSize = pageSize });

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("Page", failure.PropertyName);
        Assert.Equal("Page is out of range.", failure.ErrorMessage);
        Assert.Equal("PageOutOfRange", failure.ErrorCode);
    }

    [Fact]
    public async Task ValidateAsync_PageAndPageSizeBothInvalid_ReportsBoth()
    {
        ValidationResult result = await _validator.ValidateAsync(new ListTechniciansDto { Page = 0, PageSize = 0 });

        Assert.Equal(["Page", "PageSize"], result.Errors.Select(failure => failure.PropertyName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Bob")]
    public async Task ValidateAsync_AnyName_IsValid(string? name)
    {
        ValidationResult result = await _validator.ValidateAsync(new ListTechniciansDto { Name = name });

        Assert.True(result.IsValid);
    }
}
