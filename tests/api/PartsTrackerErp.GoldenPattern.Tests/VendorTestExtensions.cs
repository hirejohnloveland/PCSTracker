using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Tests;

internal static class VendorTestExtensions
{
    /// <summary>
    /// Gives a vendor the id RavenDB assigns when it stores one. The setter is private, and the id stays null until
    /// the vendor is stored.
    /// </summary>
    public static Vendor WithId(this Vendor vendor, string id)
    {
        typeof(Vendor).GetProperty(nameof(Vendor.Id))!.SetValue(vendor, id);

        return vendor;
    }
}
