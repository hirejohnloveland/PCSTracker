using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;

public interface IVendorsApplicationService
{
    Task<ServiceResponse<VendorResponseDto>> AddAsync(string name, CancellationToken cancellationToken = default);

    Task<ServiceResponse<VendorResponseDto>> ActivateAsync(string id, CancellationToken cancellationToken = default);

    Task<ServiceResponse<VendorResponseDto>> DeactivateAsync(string id, CancellationToken cancellationToken = default);
}
