namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

/// <summary>
/// The details of a refrigerant log entry, supplied to create one or to replace the details of an existing one. The date,
/// technician, refrigerant type, amount and notes are required. The technician must be the full name of an existing
/// technician, the refrigerant type must be one of the configured refrigerant types, and the amount must be greater than
/// zero. A ticket number, job name, customer or vendor invoice ref that is left out or blank is stored as none. A
/// customer that is given must be the name of an existing customer.
/// </summary>
public sealed class RefrigerantLogEntryDto
{
    public required DateOnly Date { get; set; }

    public required string Technician { get; set; }

    public required string RefrigerantType { get; set; }

    public required decimal AmountLbs { get; set; }

    public required string Notes { get; set; }

    public string? TicketNumber { get; set; }

    public string? JobName { get; set; }

    public string? Customer { get; set; }

    public string? VendorInvoiceRef { get; set; }
}
