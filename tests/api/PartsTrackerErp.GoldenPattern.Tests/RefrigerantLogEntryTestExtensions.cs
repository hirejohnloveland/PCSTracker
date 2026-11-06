using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Tests;

internal static class RefrigerantLogEntryTestExtensions
{
    /// <summary>
    /// Gives an entry the id RavenDB assigns when it stores one. The setter is private, and the id stays null until the
    /// entry is stored.
    /// </summary>
    public static RefrigerantLogEntry WithId(this RefrigerantLogEntry entry, string id)
    {
        typeof(RefrigerantLogEntry).GetProperty(nameof(RefrigerantLogEntry.Id))!.SetValue(entry, id);

        return entry;
    }
}
