using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

public interface ITechnicianRepository
{
    Task<Technician?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// One page of technicians, ordered by full name and then id. <paramref name="page"/> is 1-based. A
    /// <paramref name="nameSearch"/> keeps only technicians whose full name contains every word of it, ignoring case and
    /// matching inside words. Null or blank keeps every technician. An <paramref name="active"/> of true keeps only active
    /// technicians and false keeps only inactive ones. Null keeps both.
    /// </summary>
    Task<PagedResult<Technician>> ListAsync(
        string? nameSearch,
        bool? active,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Whether a technician has exactly this full name, matching case.
    /// </summary>
    Task<bool> ExistsByNameAsync(string fullName, CancellationToken cancellationToken = default);

    Task AddAsync(Technician technician, CancellationToken cancellationToken = default);
}
