using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.ApplicationServices.Abstractions;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_ServiceResponse
{
    private static readonly DomainError NotFound = new() { Code = "VendorNotFound", Message = "Vendor 7 was not found." };
    private static readonly DomainError Inactive = new() { Code = "VendorInactive", Message = "Vendor 7 is inactive." };

    [Fact]
    public void Succeed_CarriesTheDataAndNoFailure()
    {
        var response = ServiceResponse<string>.Succeed("Acme");

        Assert.True(response.Success);
        Assert.Equal("Acme", response.Data);
        Assert.Null(response.FailureType);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public void Fail_WithATypeAndErrors_CarriesBothAndNoData()
    {
        var response = ServiceResponse<string>.Fail(ErrorType.NotFound, NotFound, Inactive);

        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal(ErrorType.NotFound, response.FailureType);
        Assert.Equal([NotFound, Inactive], response.Errors);
    }

    [Fact]
    public void Fail_WithAnEnvelope_TakesItsTypeAndErrors()
    {
        var envelope = new DomainErrorEnvelope { ErrorType = ErrorType.BusinessRuleViolation, Errors = [NotFound] };

        var response = ServiceResponse<string>.Fail(envelope);

        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal(ErrorType.BusinessRuleViolation, response.FailureType);
        Assert.Equal([NotFound], response.Errors);
    }

    [Fact]
    public void Fail_FailureTypeIsTheOneGiven_NotDerivedFromTheErrors()
    {
        var response = ServiceResponse<string>.Fail(ErrorType.Unexpected, NotFound);

        Assert.Equal(ErrorType.Unexpected, response.FailureType);
    }

    [Fact]
    public void Fail_WithNoErrors_HasAnEmptyErrorList()
    {
        var response = ServiceResponse<string>.Fail(ErrorType.Forbidden);

        Assert.False(response.Success);
        Assert.Empty(response.Errors);
    }

    [Fact]
    public void Fail_ChangingTheCallersArrayAfterwards_DoesNotChangeTheResponse()
    {
        DomainError[] errors = [NotFound];
        var response = ServiceResponse<string>.Fail(ErrorType.NotFound, errors);

        errors[0] = Inactive;

        Assert.Equal([NotFound], response.Errors);
    }

    [Fact]
    public void Constructors_AreNotPublic_SoAResponseCannotBeBothSuccessfulAndFailed()
    {
        Assert.Empty(typeof(ServiceResponse<string>).GetConstructors());
    }
}
