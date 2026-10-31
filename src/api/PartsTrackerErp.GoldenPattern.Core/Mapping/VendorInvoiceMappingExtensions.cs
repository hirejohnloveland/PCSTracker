using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Core.Mapping;

public static class VendorInvoiceMappingExtensions
{
    public static VendorInvoiceResponseDto ToResponse(this VendorInvoiceDto invoice)
    {
        return new VendorInvoiceResponseDto(invoice.VendorName, invoice.Balance);
    }
}
