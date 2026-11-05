namespace PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

public sealed class Vendor
{
    private Vendor()
    {
    }

    // Null until RavenDB assigns it when the vendor is first stored.
    public string? Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    // Null for a vendor that is not linked to QuickBooks, including every vendor stored before this property existed.
    public string? QuickbooksId { get; private set; }

    // Description, Contact, Phone and Notes are null when none was provided, including for every vendor stored before
    // these properties existed.
    public string? Description { get; private set; }

    public string? Contact { get; private set; }

    public string? Phone { get; private set; }

    public string? Notes { get; private set; }

    public static Vendor Create(
        string name,
        string? description = null,
        string? contact = null,
        string? phone = null,
        string? notes = null,
        string? quickbooksId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Vendor name is required.");
        }

        return new Vendor
        {
            Name = name.Trim(),
            IsActive = true,
            Description = NullIfBlank(description),
            Contact = NullIfBlank(contact),
            Phone = NullIfBlank(phone),
            Notes = NullIfBlank(notes),
            QuickbooksId = NullIfBlank(quickbooksId)
        };
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static string? NullIfBlank(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
