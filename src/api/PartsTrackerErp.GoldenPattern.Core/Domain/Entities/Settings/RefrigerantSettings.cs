namespace PartsTrackerErp.GoldenPattern.Core.Domain.Entities.Settings;

public sealed class RefrigerantSettings : ISettingsDocument
{
    // An array, not a list: when RavenDB loads a stored document it replaces an array outright, where it would add the
    // stored items to the defaults of a list.
    private static readonly string[] s_defaultRefrigerants =
    [
        "R-22",
        "R-410A",
        "R-404A",
        "R-407C",
        "R-448A",
        "R-449A",
        "R-134a",
        "R-507A",
        "R-32",
        "Other"
    ];

    public static string DocumentId => "settings-refrigerants";

    public IReadOnlyList<string> Refrigerants { get; private set; } = s_defaultRefrigerants;

    public void Update(IEnumerable<string> refrigerants)
    {
        Refrigerants = SettingsList.Normalize(refrigerants, "refrigerant", nameof(refrigerants));
    }
}
