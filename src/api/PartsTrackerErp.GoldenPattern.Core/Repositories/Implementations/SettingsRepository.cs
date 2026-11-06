using PartsTrackerErp.GoldenPattern.Core.Domain.Entities.Settings;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;
using Raven.Client.Documents.Session;

namespace PartsTrackerErp.GoldenPattern.Core.Repositories.Implementations;

public sealed class SettingsRepository(IAsyncDocumentSession session) : ISettingsRepository
{
    public async Task<T> GetAsync<T>(CancellationToken cancellationToken = default)
        where T : class, ISettingsDocument, new()
    {
        // Load is right here: the id is fixed and belongs to this settings type alone.
        T? settings = await session.LoadAsync<T>(T.DocumentId, cancellationToken);
        return settings ?? new T();
    }

    public Task StoreAsync<T>(T settings, CancellationToken cancellationToken = default)
        where T : class, ISettingsDocument
    {
        return session.StoreAsync(settings, T.DocumentId, cancellationToken);
    }
}
