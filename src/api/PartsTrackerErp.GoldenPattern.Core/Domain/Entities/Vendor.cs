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

    public static Vendor Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Vendor name is required.");
        }

        return new Vendor
        {
            Name = name.Trim(),
            IsActive = true
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
}
