using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

public interface IRefrigerantLogRepository
{
    Task<RefrigerantLogEntry?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// One page of entries, newest date first and then by id. <paramref name="page"/> is 1-based. A
    /// <paramref name="dateFrom"/> and a <paramref name="dateTo"/> keep only entries dated within them, inclusive. A
    /// <paramref name="technician"/> keeps only entries logged for the technician with exactly that name. A
    /// <paramref name="customerSearch"/> keeps only entries whose customer name contains every word of it, ignoring case
    /// and matching inside words. A filter that is null or blank keeps every entry.
    /// </summary>
    Task<PagedResult<RefrigerantLogEntry>> ListAsync(
        DateOnly? dateFrom,
        DateOnly? dateTo,
        string? technician,
        string? customerSearch,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(RefrigerantLogEntry entry, CancellationToken cancellationToken = default);
}
