namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

public sealed class ListRefrigerantLogEntriesDto : PagedRequestDto
{
    /// <summary>
    /// Lists only entries dated on or after this date. Null lists entries from any date.
    /// </summary>
    public DateOnly? DateFrom { get; set; }

    /// <summary>
    /// Lists only entries dated on or before this date. Null lists entries up to any date.
    /// </summary>
    public DateOnly? DateTo { get; set; }

    /// <summary>
    /// Lists only entries logged for the technician with exactly this name. Null or blank lists every technician.
    /// </summary>
    public string? Technician { get; set; }

    /// <summary>
    /// Lists only entries whose customer name contains every word of this text, ignoring case. A word matches inside the
    /// words of the name. Null or blank lists every customer.
    /// </summary>
    public string? Customer { get; set; }
}
