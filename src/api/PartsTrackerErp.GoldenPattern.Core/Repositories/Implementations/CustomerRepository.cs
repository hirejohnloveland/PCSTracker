using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;
using Raven.Client.Documents;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;

namespace PartsTrackerErp.GoldenPattern.Core.Repositories.Implementations;

public sealed class CustomerRepository(IAsyncDocumentSession session) : ICustomerRepository
{
    public async Task<Customer?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        // Not Load: Load fetches any document with this id, whatever its collection. A query by id is limited to
        // Customers and, with nothing else in the filter, reads the document directly with no index.
        return await session.Query<Customer>().Where(customer => customer.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<Customer>> ListAsync(
        string? nameSearch,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Customer> query = session.Query<Customer>().Statistics(out QueryStatistics statistics);

        string[] words = SplitIntoWords(nameSearch);
        if (words.Length > 0)
        {
            // RavenDB cannot run string.Contains in a query. A full-text search with a wildcard on each side of every
            // word matches inside the words of the name, and the And operator requires every word to match.
            string terms = string.Join(' ', words.Select(word => $"*{word}*"));
            query = query.Search(customer => customer.Name, terms, @operator: SearchOperator.And);
        }

        List<Customer> customers = await query
            .OrderBy(customer => customer.Name)
            .ThenBy(customer => customer.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Customer>(customers, (int)statistics.TotalResults);
    }

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        return session.StoreAsync(customer, cancellationToken);
    }

    // Anything that is not a letter or digit separates words. The search analyzer ignores it too, and dropping it keeps
    // the characters of RavenDB's search syntax, such as * ? and quotes, out of the query.
    private static string[] SplitIntoWords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        string separated = string.Concat(text.Select(character => char.IsLetterOrDigit(character) ? character : ' '));
        return separated.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}
