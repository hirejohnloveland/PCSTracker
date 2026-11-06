using FluentValidation.Results;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Validation.Validators;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.Validation.Validators;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_CustomerDtoValidator
{
    private readonly CustomerDtoValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_OnlyAName_IsValid()
    {
        ValidationResult result = await _validator.ValidateAsync(new CustomerDto { Name = "Acme" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_AllFieldsAtTheirLimits_IsValid()
    {
        var request = new CustomerDto
        {
            Name = new string('n', CustomerDtoValidator.NameMaxLength),
            Description = new string('d', CustomerDtoValidator.DescriptionMaxLength),
            Contact = new string('c', CustomerDtoValidator.ContactMaxLength),
            Phone = "214-309-2915",
            Notes = new string('o', CustomerDtoValidator.NotesMaxLength),
            QuickbooksId = new string('9', CustomerDtoValidator.QuickbooksIdMaxLength)
        };

        ValidationResult result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_BlankName_FailsOnName(string name)
    {
        ValidationResult result = await _validator.ValidateAsync(new CustomerDto { Name = name });

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("Name", failure.PropertyName);
        Assert.Equal("Customer name is required.", failure.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_NameOverTheLimit_FailsOnName()
    {
        var request = new CustomerDto { Name = new string('n', CustomerDtoValidator.NameMaxLength + 1) };

        ValidationResult result = await _validator.ValidateAsync(request);

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("Name", failure.PropertyName);
        Assert.Equal("Customer name must be 200 characters or fewer.", failure.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_DescriptionOverTheLimit_FailsOnDescription()
    {
        var request = new CustomerDto
        {
            Name = "Acme",
            Description = new string('d', CustomerDtoValidator.DescriptionMaxLength + 1)
        };

        ValidationResult result = await _validator.ValidateAsync(request);

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("Description", failure.PropertyName);
        Assert.Equal("Description must be 500 characters or fewer.", failure.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_ContactOverTheLimit_FailsOnContact()
    {
        var request = new CustomerDto
        {
            Name = "Acme",
            Contact = new string('c', CustomerDtoValidator.ContactMaxLength + 1)
        };

        ValidationResult result = await _validator.ValidateAsync(request);

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("Contact", failure.PropertyName);
        Assert.Equal("Contact must be 200 characters or fewer.", failure.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_NotesOverTheLimit_FailsOnNotes()
    {
        var request = new CustomerDto
        {
            Name = "Acme",
            Notes = new string('o', CustomerDtoValidator.NotesMaxLength + 1)
        };

        ValidationResult result = await _validator.ValidateAsync(request);

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("Notes", failure.PropertyName);
        Assert.Equal("Notes must be 4000 characters or fewer.", failure.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_QuickbooksIdOverTheLimit_FailsOnQuickbooksId()
    {
        var request = new CustomerDto
        {
            Name = "Acme",
            QuickbooksId = new string('9', CustomerDtoValidator.QuickbooksIdMaxLength + 1)
        };

        ValidationResult result = await _validator.ValidateAsync(request);

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("QuickbooksId", failure.PropertyName);
        Assert.Equal("QuickBooks Id must be 50 characters or fewer.", failure.ErrorMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("2143092915")]
    [InlineData("(214) 309-2915")]
    [InlineData("214.309.2915")]
    [InlineData("1-214-309-2915")]
    [InlineData("+1 214 309 2915")]
    [InlineData("214-309-2915 x123")]
    [InlineData("214-309-2915 ext. 123")]
    [InlineData("+1 416 392 2489")]
    public async Task ValidateAsync_ValidUsOrCanadianPhone_IsValid(string? phone)
    {
        ValidationResult result = await _validator.ValidateAsync(new CustomerDto { Name = "Acme", Phone = phone });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("call me")]
    [InlineData("1-800-FLOWERS")]
    [InlineData("214-309-2915 x")]
    [InlineData("-2143092915")]
    [InlineData("214-309-2915; DROP")]
    [InlineData("1-214-309-29156314-1")]
    [InlineData("5550100")]
    [InlineData("309-2915")]
    [InlineData("123-456-7890")]
    [InlineData("+44 20 7946 0958")]
    public async Task ValidateAsync_PhoneThatIsNotAValidUsOrCanadianNumber_FailsOnPhone(string phone)
    {
        ValidationResult result = await _validator.ValidateAsync(new CustomerDto { Name = "Acme", Phone = phone });

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("Phone", failure.PropertyName);
        Assert.Equal(CustomerDtoValidator.PhoneMessage, failure.ErrorMessage);
    }
    [Fact]
    public async Task ValidateAsync_PhoneOverTheLimit_FailsOnPhone()
    {
        var request = new CustomerDto { Name = "Acme", Phone = new string('5', CustomerDtoValidator.PhoneMaxLength + 1) };

        ValidationResult result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, failure =>
            failure.PropertyName == "Phone" && failure.ErrorMessage == "Phone must be 50 characters or fewer.");
    }
}
