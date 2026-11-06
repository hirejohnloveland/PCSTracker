using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;

public interface ICustomersApplicationService
{
    Task<ServiceResponse<CustomerResponseDto>> AddAsync(CustomerDto request, CancellationToken cancellationToken = default);

    Task<ServiceResponse<CustomerResponseDto>> UpdateAsync(
        string id,
        CustomerDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<CustomerResponseDto>> GetAsync(string id, CancellationToken cancellationToken = default);

    Task<ServiceResponse<PagedResponseDto<CustomerResponseDto>>> ListAsync(
        ListCustomersDto request,
        CancellationToken cancellationToken = default);
}
