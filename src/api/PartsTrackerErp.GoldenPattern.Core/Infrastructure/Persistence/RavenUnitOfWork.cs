using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using Raven.Client.Documents.Session;

namespace PartsTrackerErp.GoldenPattern.Core.Infrastructure.Persistence;

public class RavenUnitOfWork(
    IAsyncDocumentSession session) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return session.SaveChangesAsync(cancellationToken);
    }
}
