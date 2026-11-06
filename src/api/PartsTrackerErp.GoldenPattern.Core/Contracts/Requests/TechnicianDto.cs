namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

/// <summary>
/// The details of a technician, supplied to create one or to replace the details of an existing one. A phone, notes or
/// QuickBooks id that is left out or blank is stored as none. The full name and the role are required, and the role must
/// be one of the configured technician roles. A technician is active unless <see cref="Active"/> is false.
/// </summary>
public sealed class TechnicianDto
{
    public required string FullName { get; set; }

    public required string Role { get; set; }

    public bool Active { get; set; } = true;

    public string? Phone { get; set; }

    public string? Notes { get; set; }

    public string? QuickbooksId { get; set; }
}
