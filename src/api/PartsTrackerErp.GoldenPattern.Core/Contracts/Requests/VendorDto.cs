namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

/// <summary>
/// The details of a vendor, supplied to create one or to replace the details of an existing one. A detail that is left
/// out or blank is stored as none, except the name, which is required.
/// </summary>
public sealed class VendorDto
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? Contact { get; set; }

    public string? Phone { get; set; }

    public string? Notes { get; set; }

    public string? QuickbooksId { get; set; }
}
