using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;
using Raven.Client.Documents.Session;

namespace PartsTrackerErp.GoldenPattern.Core.Repositories.Implementations;

public sealed class VendorRepository(IAsyncDocumentSession session) : IVendorRepository
{
    public Task<Vendor?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return session.LoadAsync<Vendor?>(id, cancellationToken);
    }

    public Task AddAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        return session.StoreAsync(vendor, cancellationToken);
    }
}
