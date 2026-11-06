using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.ApplicationServices.Implementations;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_QuickbooksApplicationService
{
    private readonly QuickbooksApplicationService _service = new();

    [Fact]
    public async Task SyncVendorsAsync_FailsAsNotImplemented()
    {
        ServiceResponse<QuickbooksSyncResponseDto> response = await _service.SyncVendorsAsync();

        Assert.False(response.Success);
        Assert.Equal(ErrorType.BusinessRuleViolation, response.FailureType);
        DomainError error = Assert.Single(response.Errors);
        Assert.Equal("QuickbooksSyncNotImplemented", error.Code);
        Assert.Equal("This feature is not implemented yet.", error.Message);
    }

    [Fact]
    public async Task SyncCustomersAsync_FailsAsNotImplemented()
    {
        ServiceResponse<QuickbooksSyncResponseDto> response = await _service.SyncCustomersAsync();

        Assert.False(response.Success);
        Assert.Equal(ErrorType.BusinessRuleViolation, response.FailureType);
        DomainError error = Assert.Single(response.Errors);
        Assert.Equal("QuickbooksSyncNotImplemented", error.Code);
        Assert.Equal("This feature is not implemented yet.", error.Message);
    }

    [Fact]
    public async Task SyncTechniciansAsync_FailsAsNotImplemented()
    {
        ServiceResponse<QuickbooksSyncResponseDto> response = await _service.SyncTechniciansAsync();

        Assert.False(response.Success);
        Assert.Equal(ErrorType.BusinessRuleViolation, response.FailureType);
        DomainError error = Assert.Single(response.Errors);
        Assert.Equal("QuickbooksSyncNotImplemented", error.Code);
        Assert.Equal("This feature is not implemented yet.", error.Message);
    }
}
