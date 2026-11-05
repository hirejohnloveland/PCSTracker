using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;
using Raven.Client.Documents;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;

namespace PartsTrackerErp.GoldenPattern.Core.Repositories.Implementations;

public sealed class VendorRepository(IAsyncDocumentSession session) : IVendorRepository
{
    public async Task<Vendor?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        // Not Load: Load fetches any document with this id, whatever its collection. A query by id is limited to
        // Vendors and, with nothing else in the filter, reads the document directly with no index.
        return await session.Query<Vendor>().Where(vendor => vendor.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<Vendor>> ListAsync(
        string? nameSearch,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Vendor> query = session.Query<Vendor>().Statistics(out QueryStatistics statistics);

        string[] words = SplitIntoWords(nameSearch);
        if (words.Length > 0)
        {
            // RavenDB cannot run string.Contains in a query. A full-text search with a wildcard on each side of every
            // word matches inside the words of the name, and the And operator requires every word to match.
            string terms = string.Join(' ', words.Select(word => $"*{word}*"));
            query = query.Search(vendor => vendor.Name, terms, @operator: SearchOperator.And);
        }

        List<Vendor> vendors = await query
            .OrderBy(vendor => vendor.Name)
            .ThenBy(vendor => vendor.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Vendor>(vendors, (int)statistics.TotalResults);
    }

    public Task AddAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        return session.StoreAsync(vendor, cancellationToken);
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
