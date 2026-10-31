using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Core.Mapping;

public static class VendorMappingExtensions
{
    public static VendorResponseDto ToResponse(this Vendor vendor)
    {
        return new VendorResponseDto(vendor.Id, vendor.Name, vendor.IsActive);
    }
}
