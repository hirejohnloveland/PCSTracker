using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// One page of customers, ordered by name and then id. <paramref name="page"/> is 1-based. A
    /// <paramref name="nameSearch"/> keeps only customers whose name contains every word of it, ignoring case and
    /// matching inside words. Null or blank keeps every customer.
    /// </summary>
    Task<PagedResult<Customer>> ListAsync(
        string? nameSearch,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
}
