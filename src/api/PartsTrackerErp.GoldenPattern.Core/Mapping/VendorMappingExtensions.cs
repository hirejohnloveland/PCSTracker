using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Core.Mapping;

public static class VendorMappingExtensions
{
    public static VendorResponseDto ToResponseDto(this Vendor vendor)
    {
        string id = vendor.Id
            ?? throw new InvalidOperationException("A vendor has no id until it is stored, and a response describes a stored vendor.");

        return new VendorResponseDto(
            Id: id,
            Name: vendor.Name,
            IsActive: vendor.IsActive,
            QuickbooksId: vendor.QuickbooksId);
    }
}
