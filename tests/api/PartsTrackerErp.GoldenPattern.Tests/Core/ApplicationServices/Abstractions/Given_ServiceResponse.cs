using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.ApplicationServices.Abstractions;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_ServiceResponse
{
    private static readonly DomainError s_notFound = new() { Code = "VendorNotFound", Message = "Vendor 7 was not found." };
    private static readonly DomainError s_inactive = new() { Code = "VendorInactive", Message = "Vendor 7 is inactive." };

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
        var response = ServiceResponse<string>.Fail(ErrorType.NotFound, s_notFound, s_inactive);

        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal(ErrorType.NotFound, response.FailureType);
        Assert.Equal([s_notFound, s_inactive], response.Errors);
    }

    [Fact]
    public void Fail_WithAnEnvelope_TakesItsTypeAndErrors()
    {
        var envelope = new DomainErrorEnvelope { ErrorType = ErrorType.BusinessRuleViolation, Errors = [s_notFound] };

        var response = ServiceResponse<string>.Fail(envelope);

        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal(ErrorType.BusinessRuleViolation, response.FailureType);
        Assert.Equal([s_notFound], response.Errors);
    }

    [Fact]
    public void Fail_FailureTypeIsTheOneGiven_NotDerivedFromTheErrors()
    {
        var response = ServiceResponse<string>.Fail(ErrorType.Unexpected, s_notFound);

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
        DomainError[] errors = [s_notFound];
        var response = ServiceResponse<string>.Fail(ErrorType.NotFound, errors);

        errors[0] = s_inactive;

        Assert.Equal([s_notFound], response.Errors);
    }

    [Fact]
    public void Constructors_AreNotPublic_SoAResponseCannotBeBothSuccessfulAndFailed()
    {
        Assert.Empty(typeof(ServiceResponse<string>).GetConstructors());
    }
}
