namespace PartsTrackerErp.GoldenPattern.Core.Domain.Entities.Settings;

public sealed class TechnicianRoleSettings : ISettingsDocument
{
    // An array, not a list: when RavenDB loads a stored document it replaces an array outright, where it would add the
    // stored items to the defaults of a list.
    private static readonly string[] s_defaultRoles = ["Service Tech", "Installer", "Other"];

    public static string DocumentId => "settings-technician-roles";

    // The job titles a technician can have.
    public IReadOnlyList<string> Roles { get; private set; } = s_defaultRoles;

    public void Update(IEnumerable<string> roles)
    {
        Roles = SettingsList.Normalize(roles, "role", nameof(roles));
    }
}
