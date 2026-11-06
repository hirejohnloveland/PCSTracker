using PartsTrackerErp.GoldenPattern.Core.Domain.Entities.Settings;

namespace PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

public interface ISettingsRepository
{
    /// <summary>
    /// The stored settings of type <typeparamref name="T"/>, or a new instance with its default values when none are
    /// stored. A default instance is not stored until <see cref="StoreAsync{T}"/> is called with it.
    /// </summary>
    Task<T> GetAsync<T>(CancellationToken cancellationToken = default)
        where T : class, ISettingsDocument, new();

    Task StoreAsync<T>(T settings, CancellationToken cancellationToken = default)
        where T : class, ISettingsDocument;
}
