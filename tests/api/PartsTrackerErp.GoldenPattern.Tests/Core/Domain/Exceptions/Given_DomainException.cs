using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.Domain.Exceptions;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_DomainException
{
    private static readonly DomainError s_notFound = new() { Code = "VendorNotFound", Message = "Vendor 7 was not found." };
    private static readonly DomainError s_inactive = new() { Code = "VendorInactive", Message = "Vendor 7 is inactive." };

    [Fact]
    public void Constructor_WithErrorTypeAndErrors_CarriesBothOnTheEnvelope()
    {
        var exception = new DomainException(ErrorType.BusinessRuleViolation, s_notFound, s_inactive);

        Assert.Equal(ErrorType.BusinessRuleViolation, exception.ErrorEnvelope.ErrorType);
        Assert.Equal([s_notFound, s_inactive], exception.ErrorEnvelope.Errors);
        Assert.Equal(exception.ErrorEnvelope.Errors, exception.Errors);
    }

    [Fact]
    public void Constructor_WithAnErrorList_CarriesEveryError()
    {
        List<DomainError> errors = [s_notFound, s_inactive];

        var exception = new DomainException(ErrorType.Validation, errors);

        Assert.Equal(ErrorType.Validation, exception.ErrorEnvelope.ErrorType);
        Assert.Equal(errors, exception.Errors);
    }

    [Fact]
    public void Constructor_WithAnEnvelope_ExposesThatEnvelope()
    {
        var envelope = new DomainErrorEnvelope { ErrorType = ErrorType.Forbidden, Errors = [s_notFound] };

        var exception = new DomainException(envelope);

        Assert.Same(envelope, exception.ErrorEnvelope);
    }

    [Fact]
    public void Constructor_WithAnInnerException_KeepsIt()
    {
        var inner = new InvalidOperationException("Root cause.");

        var exception = new DomainException(ErrorType.Unexpected, [s_notFound], inner);

        Assert.Same(inner, exception.InnerException);
        Assert.Equal(ErrorType.Unexpected, exception.ErrorEnvelope.ErrorType);
    }

    [Fact]
    public void Message_ListsEachErrorCodeAndMessage()
    {
        var exception = new DomainException(ErrorType.BusinessRuleViolation, s_notFound, s_inactive);

        Assert.Equal(
            "[VendorNotFound] Vendor 7 was not found. | [VendorInactive] Vendor 7 is inactive.",
            exception.Message);
    }

    [Fact]
    public void Message_WithNoErrors_SaysSo()
    {
        var exception = new DomainException(ErrorType.Unexpected);

        Assert.Empty(exception.Errors);
        Assert.Equal("DomainException with no errors.", exception.Message);
    }
}
