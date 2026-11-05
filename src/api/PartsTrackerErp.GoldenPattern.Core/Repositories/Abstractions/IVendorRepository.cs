using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

public interface IVendorRepository
{
    Task<Vendor?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// One page of vendors, ordered by name and then id. <paramref name="page"/> is 1-based. A
    /// <paramref name="nameSearch"/> keeps only vendors whose name contains every word of it, ignoring case and
    /// matching inside words. Null or blank keeps every vendor.
    /// </summary>
    Task<PagedResult<Vendor>> ListAsync(
        string? nameSearch,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(Vendor vendor, CancellationToken cancellationToken = default);
}
