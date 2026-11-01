using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

public interface IVendorRepository
{
    Task<Vendor?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// One page of vendors, ordered by name and then id. <paramref name="page"/> is 1-based. A
    /// <paramref name="namePrefix"/> keeps only vendors whose name starts with it, ignoring case. Null or blank keeps
    /// every vendor.
    /// </summary>
    Task<PagedResult<Vendor>> ListAsync(
        string? namePrefix,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(Vendor vendor, CancellationToken cancellationToken = default);
}
