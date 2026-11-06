namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

public sealed class ListTechniciansDto : PagedRequestDto
{
    /// <summary>
    /// Lists only technicians whose full name contains every word of this text, ignoring case. A word matches inside the
    /// words of the name, so "ost" finds "Bob Osborne". Null or blank lists every technician.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// True lists only active technicians and false lists only inactive ones. Null lists both.
    /// </summary>
    public bool? Active { get; set; }
}
