using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;

public sealed class QuickbooksApplicationService : IQuickbooksApplicationService
{
    public Task<ServiceResponse<QuickbooksSyncResponseDto>> SyncVendorsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ServiceResponse<QuickbooksSyncResponseDto>.Fail(
            ErrorType.BusinessRuleViolation,
            QuickbooksErrors.SyncNotImplemented()));
    }

    public Task<ServiceResponse<QuickbooksSyncResponseDto>> SyncCustomersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ServiceResponse<QuickbooksSyncResponseDto>.Fail(
            ErrorType.BusinessRuleViolation,
            QuickbooksErrors.SyncNotImplemented()));
    }

    public Task<ServiceResponse<QuickbooksSyncResponseDto>> SyncTechniciansAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ServiceResponse<QuickbooksSyncResponseDto>.Fail(
            ErrorType.BusinessRuleViolation,
            QuickbooksErrors.SyncNotImplemented()));
    }
}
