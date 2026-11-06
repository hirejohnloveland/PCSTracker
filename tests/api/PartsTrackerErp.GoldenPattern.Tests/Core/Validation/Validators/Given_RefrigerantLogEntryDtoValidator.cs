using FluentValidation.Results;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities.Settings;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Validation.Validators;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.Validation.Validators;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_RefrigerantLogEntryDtoValidator
{
    private readonly Mock<ISettingsRepository> _settings = new();
    private readonly Mock<ITechnicianRepository> _technicians = new();
    private readonly Mock<ICustomerRepository> _customers = new();
    private readonly RefrigerantLogEntryDtoValidator _validator;

    public Given_RefrigerantLogEntryDtoValidator()
    {
        // The default refrigerant types include R-404A and R-22.
        _settings
            .Setup(s => s.GetAsync<RefrigerantSettings>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefrigerantSettings());
        _technicians.Setup(t => t.ExistsByNameAsync("Bob Osborne", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _customers.Setup(c => c.ExistsByNameAsync("Metro Food Market - Bronx", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _validator = new RefrigerantLogEntryDtoValidator(_settings.Object, _technicians.Object, _customers.Object);
    }

    [Fact]
    public async Task ValidateAsync_OnlyTheRequiredFields_IsValid()
    {
        ValidationResult result = await _validator.ValidateAsync(NewRequest());

        Assert.True(result.IsValid);
        _customers.Verify(c => c.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_AllFieldsAtTheirLimits_IsValid()
    {
        RefrigerantLogEntryDto request = NewRequest();
        request.Customer = "  Metro Food Market - Bronx  ";
        request.TicketNumber = new string('t', RefrigerantLogEntryDtoValidator.TicketNumberMaxLength);
        request.JobName = new string('j', RefrigerantLogEntryDtoValidator.JobNameMaxLength);
        request.VendorInvoiceRef = new string('v', RefrigerantLogEntryDtoValidator.VendorInvoiceRefMaxLength);
        request.Notes = new string('n', RefrigerantLogEntryDtoValidator.NotesMaxLength);

        ValidationResult result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_NoDate_FailsOnDate()
    {
        RefrigerantLogEntryDto request = NewRequest();
        request.Date = default;

        ValidationResult result = await _validator.ValidateAsync(request);

        AssertSingleFailure(result, "Date", "Date is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_BlankTechnician_FailsOnTechnician(string technician)
    {
        RefrigerantLogEntryDto request = NewRequest();
        request.Technician = technician;

        ValidationResult result = await _validator.ValidateAsync(request);

        AssertSingleFailure(result, "Technician", "Technician is required.");
    }

    [Fact]
    public async Task ValidateAsync_UnknownTechnician_FailsOnTechnician()
    {
        RefrigerantLogEntryDto request = NewRequest();
        request.Technician = "Nobody";

        ValidationResult result = await _validator.ValidateAsync(request);

        AssertSingleFailure(result, "Technician", RefrigerantLogEntryDtoValidator.TechnicianMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_BlankRefrigerantType_FailsOnRefrigerantType(string refrigerantType)
    {
        RefrigerantLogEntryDto request = NewRequest();
        request.RefrigerantType = refrigerantType;

        ValidationResult result = await _validator.ValidateAsync(request);

        AssertSingleFailure(result, "RefrigerantType", "Refrigerant type is required.");
    }

    [Theory]
    [InlineData("R-9999")]
    [InlineData("r-22")]
    public async Task ValidateAsync_RefrigerantTypeNotInSettings_FailsOnRefrigerantType(string refrigerantType)
    {
        RefrigerantLogEntryDto request = NewRequest();
        request.RefrigerantType = refrigerantType;

        ValidationResult result = await _validator.ValidateAsync(request);

        AssertSingleFailure(result, "RefrigerantType", RefrigerantLogEntryDtoValidator.RefrigerantTypeMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1.5)]
    public async Task ValidateAsync_AmountNotGreaterThanZero_FailsOnAmountLbs(double amount)
    {
        RefrigerantLogEntryDto request = NewRequest();
        request.AmountLbs = (decimal)amount;

        ValidationResult result = await _validator.ValidateAsync(request);

        AssertSingleFailure(result, "AmountLbs", "Amount used must be greater than zero.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_BlankNotes_FailsOnNotes(string notes)
    {
        RefrigerantLogEntryDto request = NewRequest();
        request.Notes = notes;

        ValidationResult result = await _validator.ValidateAsync(request);

        AssertSingleFailure(result, "Notes", "Notes are required.");
    }

    [Fact]
    public async Task ValidateAsync_NotesOverTheLimit_FailsOnNotes()
    {
        RefrigerantLogEntryDto request = NewRequest();
        request.Notes = new string('n', RefrigerantLogEntryDtoValidator.NotesMaxLength + 1);

        ValidationResult result = await _validator.ValidateAsync(request);

        AssertSingleFailure(result, "Notes", "Notes must be 4000 characters or fewer.");
    }

    [Fact]
    public async Task ValidateAsync_TicketNumberOverTheLimit_FailsOnTicketNumber()
    {
        RefrigerantLogEntryDto request = NewRequest();
        request.TicketNumber = new string('t', RefrigerantLogEntryDtoValidator.TicketNumberMaxLength + 1);

        ValidationResult result = await _validator.ValidateAsync(request);

        AssertSingleFailure(result, "TicketNumber", "Ticket number must be 50 characters or fewer.");
    }

    [Fact]
    public async Task ValidateAsync_JobNameOverTheLimit_FailsOnJobName()
    {
        RefrigerantLogEntryDto request = NewRequest();
        request.JobName = new string('j', RefrigerantLogEntryDtoValidator.JobNameMaxLength + 1);

        ValidationResult result = await _validator.ValidateAsync(request);

        AssertSingleFailure(result, "JobName", "Job name must be 200 characters or fewer.");
    }

    [Fact]
    public async Task ValidateAsync_VendorInvoiceRefOverTheLimit_FailsOnVendorInvoiceRef()
    {
        RefrigerantLogEntryDto request = NewRequest();
        request.VendorInvoiceRef = new string('v', RefrigerantLogEntryDtoValidator.VendorInvoiceRefMaxLength + 1);

        ValidationResult result = await _validator.ValidateAsync(request);

        AssertSingleFailure(result, "VendorInvoiceRef", "Vendor invoice ref must be 100 characters or fewer.");
    }

    [Fact]
    public async Task ValidateAsync_UnknownCustomer_FailsOnCustomer()
    {
        RefrigerantLogEntryDto request = NewRequest();
        request.Customer = "Nobody Inc";

        ValidationResult result = await _validator.ValidateAsync(request);

        AssertSingleFailure(result, "Customer", RefrigerantLogEntryDtoValidator.CustomerMessage);
    }

    [Fact]
    public async Task ValidateAsync_BlankCustomer_IsValidAndIsNotLookedUp()
    {
        RefrigerantLogEntryDto request = NewRequest();
        request.Customer = "   ";

        ValidationResult result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
        _customers.Verify(c => c.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static RefrigerantLogEntryDto NewRequest()
    {
        return new RefrigerantLogEntryDto
        {
            Date = new DateOnly(2026, 5, 15),
            Technician = "Bob Osborne",
            RefrigerantType = "R-22",
            AmountLbs = 6m,
            Notes = "Compressor replacement"
        };
    }

    private static void AssertSingleFailure(ValidationResult result, string property, string message)
    {
        ValidationFailure failure = Assert.Single(result.Errors);
        Assert.Equal(property, failure.PropertyName);
        Assert.Equal(message, failure.ErrorMessage);
    }
}
