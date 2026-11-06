using FluentValidation.Results;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities.Settings;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Validation.Validators;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.Validation.Validators;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_TechnicianDtoValidator
{
    private readonly Mock<ISettingsRepository> _settings = new();
    private readonly TechnicianDtoValidator _validator;

    public Given_TechnicianDtoValidator()
    {
        // The default roles are Service Tech, Installer and Other.
        _settings
            .Setup(s => s.GetAsync<TechnicianRoleSettings>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TechnicianRoleSettings());
        _validator = new TechnicianDtoValidator(_settings.Object);
    }

    [Fact]
    public async Task ValidateAsync_OnlyAFullNameAndARole_IsValid()
    {
        ValidationResult result = await _validator.ValidateAsync(
            new TechnicianDto { FullName = "Bob Osborne", Role = "Service Tech" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_AllFieldsAtTheirLimits_IsValid()
    {
        var request = new TechnicianDto
        {
            FullName = new string('n', TechnicianDtoValidator.FullNameMaxLength),
            Role = "Installer",
            Phone = "214-309-2915",
            Notes = new string('o', TechnicianDtoValidator.NotesMaxLength),
            QuickbooksId = new string('9', TechnicianDtoValidator.QuickbooksIdMaxLength)
        };

        ValidationResult result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_BlankFullName_FailsOnFullName(string fullName)
    {
        ValidationResult result = await _validator.ValidateAsync(
            new TechnicianDto { FullName = fullName, Role = "Service Tech" });

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("FullName", failure.PropertyName);
        Assert.Equal("Full name is required.", failure.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_FullNameOverTheLimit_FailsOnFullName()
    {
        var request = new TechnicianDto
        {
            FullName = new string('n', TechnicianDtoValidator.FullNameMaxLength + 1),
            Role = "Service Tech"
        };

        ValidationResult result = await _validator.ValidateAsync(request);

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("FullName", failure.PropertyName);
        Assert.Equal("Full name must be 200 characters or fewer.", failure.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_BlankRole_FailsOnRoleAsRequired(string role)
    {
        ValidationResult result = await _validator.ValidateAsync(new TechnicianDto { FullName = "Bob Osborne", Role = role });

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("Role", failure.PropertyName);
        Assert.Equal("Role is required.", failure.ErrorMessage);
    }

    [Theory]
    [InlineData("Service Tech")]
    [InlineData("Installer")]
    [InlineData("Other")]
    [InlineData("  Installer  ")]
    public async Task ValidateAsync_RoleInTheSettings_IsValid(string role)
    {
        ValidationResult result = await _validator.ValidateAsync(new TechnicianDto { FullName = "Bob Osborne", Role = role });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Plumber")]
    [InlineData("service tech")]
    public async Task ValidateAsync_RoleNotInTheSettings_FailsOnRole(string role)
    {
        ValidationResult result = await _validator.ValidateAsync(new TechnicianDto { FullName = "Bob Osborne", Role = role });

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("Role", failure.PropertyName);
        Assert.Equal(TechnicianDtoValidator.RoleMessage, failure.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_UsesTheStoredRolesAndPassesTheToken()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        var settings = new TechnicianRoleSettings();
        settings.Update(["Lead"]);
        _settings.Setup(s => s.GetAsync<TechnicianRoleSettings>(token)).ReturnsAsync(settings);

        ValidationResult lead = await _validator.ValidateAsync(
            new TechnicianDto { FullName = "Bob Osborne", Role = "Lead" },
            token);
        ValidationResult installer = await _validator.ValidateAsync(
            new TechnicianDto { FullName = "Bob Osborne", Role = "Installer" },
            token);

        Assert.True(lead.IsValid);
        Assert.Equal("Role", Assert.Single(installer.Errors).PropertyName);
    }

    [Fact]
    public async Task ValidateAsync_NotesOverTheLimit_FailsOnNotes()
    {
        var request = new TechnicianDto
        {
            FullName = "Bob Osborne",
            Role = "Service Tech",
            Notes = new string('o', TechnicianDtoValidator.NotesMaxLength + 1)
        };

        ValidationResult result = await _validator.ValidateAsync(request);

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("Notes", failure.PropertyName);
        Assert.Equal("Notes must be 4000 characters or fewer.", failure.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_QuickbooksIdOverTheLimit_FailsOnQuickbooksId()
    {
        var request = new TechnicianDto
        {
            FullName = "Bob Osborne",
            Role = "Service Tech",
            QuickbooksId = new string('9', TechnicianDtoValidator.QuickbooksIdMaxLength + 1)
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
        ValidationResult result = await _validator.ValidateAsync(
            new TechnicianDto { FullName = "Bob Osborne", Role = "Service Tech", Phone = phone });

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
        ValidationResult result = await _validator.ValidateAsync(
            new TechnicianDto { FullName = "Bob Osborne", Role = "Service Tech", Phone = phone });

        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal("Phone", failure.PropertyName);
        Assert.Equal(TechnicianDtoValidator.PhoneMessage, failure.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_PhoneOverTheLimit_FailsOnPhone()
    {
        var request = new TechnicianDto
        {
            FullName = "Bob Osborne",
            Role = "Service Tech",
            Phone = new string('5', TechnicianDtoValidator.PhoneMaxLength + 1)
        };

        ValidationResult result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, failure =>
            failure.PropertyName == "Phone" && failure.ErrorMessage == "Phone must be 50 characters or fewer.");
    }
}
