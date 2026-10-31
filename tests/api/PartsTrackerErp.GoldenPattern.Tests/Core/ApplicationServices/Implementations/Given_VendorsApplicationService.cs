using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.ApplicationServices.Implementations;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_VendorsApplicationService
{
    private readonly Mock<IVendorRepository> repository = new();
    private readonly Mock<IUnitOfWork> unitOfWork = new();
    private readonly VendorsApplicationService service;

    public Given_VendorsApplicationService()
    {
        service = new VendorsApplicationService(repository.Object, unitOfWork.Object);
    }

    [Fact]
    public async Task AddAsync_StoresTheVendorSavesAndSucceeds()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;

        ServiceResponse<Vendor> response = await service.AddAsync("  Acme  ", token);

        Assert.True(response.Success);
        Assert.Equal("Acme", response.Data.Name);
        Assert.True(response.Data.IsActive);
        repository.Verify(r => r.AddAsync(response.Data, token), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(token), Times.Once);
    }

    [Fact]
    public async Task ActivateAsync_MissingVendor_FailsWithNotFoundAndDoesNotSave()
    {
        repository.Setup(r => r.GetAsync("7", It.IsAny<CancellationToken>())).ReturnsAsync((Vendor?)null);

        ServiceResponse<Vendor> response = await service.ActivateAsync("7");

        AssertNotFound(response, "7");
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ActivateAsync_ExistingVendor_ActivatesItSavesAndSucceeds()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        Vendor vendor = Vendor.Create("Acme");
        vendor.Deactivate();
        repository.Setup(r => r.GetAsync("7", token)).ReturnsAsync(vendor);

        ServiceResponse<Vendor> response = await service.ActivateAsync("7", token);

        Assert.True(response.Success);
        Assert.Same(vendor, response.Data);
        Assert.True(vendor.IsActive);
        unitOfWork.Verify(u => u.SaveChangesAsync(token), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_MissingVendor_FailsWithNotFoundAndDoesNotSave()
    {
        repository.Setup(r => r.GetAsync("7", It.IsAny<CancellationToken>())).ReturnsAsync((Vendor?)null);

        ServiceResponse<Vendor> response = await service.DeactivateAsync("7");

        AssertNotFound(response, "7");
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeactivateAsync_ExistingVendor_DeactivatesItSavesAndSucceeds()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        Vendor vendor = Vendor.Create("Acme");
        repository.Setup(r => r.GetAsync("7", token)).ReturnsAsync(vendor);

        ServiceResponse<Vendor> response = await service.DeactivateAsync("7", token);

        Assert.True(response.Success);
        Assert.Same(vendor, response.Data);
        Assert.False(vendor.IsActive);
        unitOfWork.Verify(u => u.SaveChangesAsync(token), Times.Once);
    }

    private static void AssertNotFound(ServiceResponse<Vendor> response, string id)
    {
        Assert.False(response.Success);
        Assert.Equal(ErrorType.NotFound, response.FailureType);
        DomainError error = Assert.Single(response.Errors);
        Assert.Equal("VendorNotFound", error.Code);
        Assert.Contains(id, error.Message);
    }
}
