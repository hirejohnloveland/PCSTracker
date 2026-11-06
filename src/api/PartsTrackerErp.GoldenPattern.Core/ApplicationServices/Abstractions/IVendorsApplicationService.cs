using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;

public interface IVendorsApplicationService
{
    Task<ServiceResponse<VendorResponseDto>> AddAsync(VendorDto request, CancellationToken cancellationToken = default);

    Task<ServiceResponse<VendorResponseDto>> UpdateAsync(
        string id,
        VendorDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<VendorResponseDto>> GetAsync(string id, CancellationToken cancellationToken = default);

    Task<ServiceResponse<PagedResponseDto<VendorResponseDto>>> ListAsync(
        ListVendorsDto request,
        CancellationToken cancellationToken = default);
}
