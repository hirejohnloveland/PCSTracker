using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;

public interface IQuickbooksApplicationService
{
    Task<ServiceResponse<QuickbooksSyncResponseDto>> SyncVendorsAsync(CancellationToken cancellationToken = default);

    Task<ServiceResponse<QuickbooksSyncResponseDto>> SyncCustomersAsync(CancellationToken cancellationToken = default);
}
