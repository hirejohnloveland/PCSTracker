using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Core.Mapping;

public static class VendorMappingExtensions
{
    public static VendorResponseDto ToResponseDto(this Vendor vendor)
    {
        return new VendorResponseDto(Id: vendor.Id, Name: vendor.Name, IsActive: vendor.IsActive);
    }
}
