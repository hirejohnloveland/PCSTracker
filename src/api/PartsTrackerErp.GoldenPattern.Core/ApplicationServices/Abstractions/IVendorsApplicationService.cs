using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;

public interface IVendorsApplicationService
{
    Task<ServiceResponse<VendorResponseDto>> AddAsync(CreateVendorDto request, CancellationToken cancellationToken = default);

    Task<ServiceResponse<VendorResponseDto>> UpdateAsync(
        string id,
        UpdateVendorDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<VendorResponseDto>> GetAsync(string id, CancellationToken cancellationToken = default);

    Task<ServiceResponse<PagedResponseDto<VendorResponseDto>>> ListAsync(
        ListVendorsDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<VendorResponseDto>> ActivateAsync(string id, CancellationToken cancellationToken = default);

    Task<ServiceResponse<VendorResponseDto>> DeactivateAsync(string id, CancellationToken cancellationToken = default);
}
