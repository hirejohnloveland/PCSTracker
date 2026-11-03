using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Mapping;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.Mapping;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_VendorMappingExtensions
{
    [Fact]
    public void ToResponseDto_CopiesTheIdNameAndActiveFlag()
    {
        Vendor vendor = Vendor.Create("Acme").WithId("vendors-1-A");

        VendorResponseDto response = vendor.ToResponseDto();

        Assert.Equal(new VendorResponseDto("vendors-1-A", "Acme", true), response);
    }

    [Fact]
    public void ToResponseDto_DeactivatedVendor_CopiesTheInactiveFlag()
    {
        Vendor vendor = Vendor.Create("Acme").WithId("vendors-1-A");
        vendor.Deactivate();

        VendorResponseDto response = vendor.ToResponseDto();

        Assert.Equal(new VendorResponseDto("vendors-1-A", "Acme", false), response);
    }

    [Fact]
    public void ToResponseDto_VendorThatIsNotStored_Throws()
    {
        Vendor vendor = Vendor.Create("Acme");

        Assert.Throws<InvalidOperationException>(() => vendor.ToResponseDto());
    }
}
