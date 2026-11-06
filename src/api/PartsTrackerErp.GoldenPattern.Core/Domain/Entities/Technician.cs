namespace PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

public sealed class Technician
{
    private Technician()
    {
    }

    // Null until RavenDB assigns it when the technician is first stored.
    public string? Id { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    // One of the roles in TechnicianRoleSettings. The entity does not check that, because the roles are stored settings.
    public string Role { get; private set; } = string.Empty;

    public bool Active { get; private set; } = true;

    // Null for a technician that is not linked to QuickBooks.
    public string? QuickbooksId { get; private set; }

    // Phone and Notes are null when none was provided.
    public string? Phone { get; private set; }

    public string? Notes { get; private set; }

    public static Technician Create(
        string fullName,
        string role,
        string? phone = null,
        string? notes = null,
        string? quickbooksId = null,
        bool active = true)
    {
        var technician = new Technician();
        technician.Update(fullName, role, phone, notes, quickbooksId, active);

        return technician;
    }

    /// <summary>
    /// Replaces every detail. A blank phone, notes or QuickBooks id clears it.
    /// </summary>
    public void Update(
        string fullName,
        string role,
        string? phone,
        string? notes,
        string? quickbooksId,
        bool active)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Technician full name is required.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Technician role is required.");
        }

        FullName = fullName.Trim();
        Role = role.Trim();
        Active = active;
        Phone = NullIfBlank(phone);
        Notes = NullIfBlank(notes);
        QuickbooksId = NullIfBlank(quickbooksId);
    }

    private static string? NullIfBlank(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
