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

    /// <summary>
    /// Gives a vendor the QuickBooks id that a vendor linked to QuickBooks has. The setter is private, and nothing in
    /// the application sets it yet.
    /// </summary>
    public static Vendor WithQuickbooksId(this Vendor vendor, string quickbooksId)
    {
        typeof(Vendor).GetProperty(nameof(Vendor.QuickbooksId))!.SetValue(vendor, quickbooksId);

        return vendor;
    }
}
