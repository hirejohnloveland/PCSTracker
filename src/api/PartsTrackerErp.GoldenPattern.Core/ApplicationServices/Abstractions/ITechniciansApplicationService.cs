using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;

public interface ITechniciansApplicationService
{
    Task<ServiceResponse<TechnicianResponseDto>> AddAsync(TechnicianDto request, CancellationToken cancellationToken = default);

    Task<ServiceResponse<TechnicianResponseDto>> UpdateAsync(
        string id,
        TechnicianDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<TechnicianResponseDto>> GetAsync(string id, CancellationToken cancellationToken = default);

    Task<ServiceResponse<PagedResponseDto<TechnicianResponseDto>>> ListAsync(
        ListTechniciansDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<TechnicianRolesResponseDto>> GetRolesAsync(CancellationToken cancellationToken = default);
}
