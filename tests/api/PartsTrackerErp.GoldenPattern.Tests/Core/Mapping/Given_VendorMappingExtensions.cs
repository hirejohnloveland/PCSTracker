using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Mapping;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.Mapping;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_VendorMappingExtensions
{
    [Fact]
    public void ToResponse_CopiesTheIdNameAndActiveFlag()
    {
        Vendor vendor = Vendor.Create("Acme");

        // Id has a private setter and stays null until the vendor is stored.
        typeof(Vendor).GetProperty(nameof(Vendor.Id))!.SetValue(vendor, "vendors/1-A");

        VendorResponseDto response = vendor.ToResponse();

        Assert.Equal(new VendorResponseDto("vendors/1-A", "Acme", true), response);
    }
}
