using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Tests;

internal static class TechnicianTestExtensions
{
    /// <summary>
    /// Gives a technician the id RavenDB assigns when it stores one. The setter is private, and the id stays null until
    /// the technician is stored.
    /// </summary>
    public static Technician WithId(this Technician technician, string id)
    {
        typeof(Technician).GetProperty(nameof(Technician.Id))!.SetValue(technician, id);

        return technician;
    }
}
