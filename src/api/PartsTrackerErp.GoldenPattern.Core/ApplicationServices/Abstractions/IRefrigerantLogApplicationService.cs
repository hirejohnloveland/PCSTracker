using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;

public interface IRefrigerantLogApplicationService
{
    Task<ServiceResponse<RefrigerantLogEntryResponseDto>> AddAsync(
        RefrigerantLogEntryDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<RefrigerantLogEntryResponseDto>> UpdateAsync(
        string id,
        RefrigerantLogEntryDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<RefrigerantLogEntryResponseDto>> GetAsync(string id, CancellationToken cancellationToken = default);

    Task<ServiceResponse<PagedResponseDto<RefrigerantLogEntryResponseDto>>> ListAsync(
        ListRefrigerantLogEntriesDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<RefrigerantTypesResponseDto>> GetRefrigerantTypesAsync(CancellationToken cancellationToken = default);
}
