using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;
using Raven.Client.Documents;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;

namespace PartsTrackerErp.GoldenPattern.Core.Repositories.Implementations;

public sealed class RefrigerantLogRepository(IAsyncDocumentSession session) : IRefrigerantLogRepository
{
    public async Task<RefrigerantLogEntry?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        // Not Load: Load fetches any document with this id, whatever its collection. A query by id is limited to
        // refrigerant log entries and, with nothing else in the filter, reads the document directly with no index.
        return await session.Query<RefrigerantLogEntry>().Where(entry => entry.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<RefrigerantLogEntry>> ListAsync(
        DateOnly? dateFrom,
        DateOnly? dateTo,
        string? technician,
        string? customerSearch,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<RefrigerantLogEntry> query = session.Query<RefrigerantLogEntry>().Statistics(out QueryStatistics statistics);

        if (dateFrom is not null)
        {
            DateOnly from = dateFrom.Value;
            query = query.Where(entry => entry.Date >= from);
        }

        if (dateTo is not null)
        {
            DateOnly to = dateTo.Value;
            query = query.Where(entry => entry.Date <= to);
        }

        if (!string.IsNullOrWhiteSpace(technician))
        {
            string name = technician.Trim();
            query = query.Where(entry => entry.TechnicianName == name, exact: true);
        }

        string[] words = SplitIntoWords(customerSearch);
        if (words.Length > 0)
        {
            // RavenDB cannot run string.Contains in a query. A full-text search with a wildcard on each side of every
            // word matches inside the words of the name, and the And operator requires every word to match.
            string terms = string.Join(' ', words.Select(word => $"*{word}*"));
            query = query.Search(entry => entry.CustomerName, terms, @operator: SearchOperator.And);
        }

        List<RefrigerantLogEntry> entries = await query
            .OrderByDescending(entry => entry.Date)
            .ThenBy(entry => entry.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<RefrigerantLogEntry>(entries, (int)statistics.TotalResults);
    }

    public Task AddAsync(RefrigerantLogEntry entry, CancellationToken cancellationToken = default)
    {
        return session.StoreAsync(entry, cancellationToken);
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
