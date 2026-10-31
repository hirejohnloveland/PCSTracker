using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;

public interface IVendorsApplicationService
{
    Task<Vendor> AddAsync(string name, CancellationToken cancellationToken = default);

    Task<Vendor?> ActivateAsync(string id, CancellationToken cancellationToken = default);

    Task<Vendor?> DeactivateAsync(string id, CancellationToken cancellationToken = default);
}
