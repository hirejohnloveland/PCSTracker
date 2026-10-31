using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Core.Mapping;

public static class VendorInvoiceMappingExtensions
{
    public static VendorInvoiceResponseDto ToResponseDto(this VendorInvoiceDto invoice)
    {
        return new VendorInvoiceResponseDto(VendorName: invoice.VendorName, Balance: invoice.Balance);
    }
}
