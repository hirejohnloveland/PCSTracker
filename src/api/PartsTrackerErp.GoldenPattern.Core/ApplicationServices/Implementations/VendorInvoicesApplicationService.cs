using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Mapping;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;

public sealed class VendorInvoicesApplicationService : IVendorInvoicesApplicationService
{
    public Task<ServiceResponse<VendorInvoiceResponseDto>> CreateAsync(VendorInvoiceDto invoice, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ServiceResponse<VendorInvoiceResponseDto>.Succeed(invoice.ToResponseDto()));
    }
}
