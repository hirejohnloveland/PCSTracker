using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;
using Raven.Client.Documents;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;

namespace PartsTrackerErp.GoldenPattern.Core.Repositories.Implementations;

public sealed class TechnicianRepository(IAsyncDocumentSession session) : ITechnicianRepository
{
    public async Task<Technician?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        // Not Load: Load fetches any document with this id, whatever its collection. A query by id is limited to
        // Technicians and, with nothing else in the filter, reads the document directly with no index.
        return await session.Query<Technician>().Where(technician => technician.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<Technician>> ListAsync(
        string? nameSearch,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Technician> query = session.Query<Technician>().Statistics(out QueryStatistics statistics);

        string[] words = SplitIntoWords(nameSearch);
        if (words.Length > 0)
        {
            // RavenDB cannot run string.Contains in a query. A full-text search with a wildcard on each side of every
            // word matches inside the words of the name, and the And operator requires every word to match.
            string terms = string.Join(' ', words.Select(word => $"*{word}*"));
            query = query.Search(technician => technician.FullName, terms, @operator: SearchOperator.And);
        }

        List<Technician> technicians = await query
            .OrderBy(technician => technician.FullName)
            .ThenBy(technician => technician.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Technician>(technicians, (int)statistics.TotalResults);
    }

    public Task AddAsync(Technician technician, CancellationToken cancellationToken = default)
    {
        return session.StoreAsync(technician, cancellationToken);
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
