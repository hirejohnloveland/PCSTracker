using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;

public interface IVendorInvoicesApplicationService
{
    Task<ServiceResponse<VendorInvoiceResponseDto>> CreateAsync(VendorInvoiceDto invoice, CancellationToken cancellationToken = default);
}
