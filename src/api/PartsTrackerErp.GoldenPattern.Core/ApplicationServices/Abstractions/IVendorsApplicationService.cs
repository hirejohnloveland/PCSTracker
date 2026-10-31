using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;

public interface IVendorsApplicationService
{
    Task<ServiceResponse<Vendor>> AddAsync(string name, CancellationToken cancellationToken = default);

    Task<ServiceResponse<Vendor>> ActivateAsync(string id, CancellationToken cancellationToken = default);

    Task<ServiceResponse<Vendor>> DeactivateAsync(string id, CancellationToken cancellationToken = default);
}
