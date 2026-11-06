namespace PartsTrackerErp.GoldenPattern.Core.Domain.Entities.Settings;

/// <summary>
/// A document of application settings. Each implementation is stored under its own fixed <see cref="DocumentId"/>, and
/// every implementation shares one Settings collection.
/// </summary>
public interface ISettingsDocument
{
    static abstract string DocumentId { get; }
}
