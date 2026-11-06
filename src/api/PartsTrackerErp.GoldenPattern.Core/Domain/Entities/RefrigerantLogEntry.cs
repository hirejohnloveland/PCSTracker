namespace PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

public sealed class RefrigerantLogEntry
{
    private RefrigerantLogEntry()
    {
    }

    // Null until RavenDB assigns it when the entry is first stored.
    public string? Id { get; private set; }

    public DateOnly Date { get; private set; }

    // The technician's name as it was when the entry was logged. It is text, not a reference, so renaming or
    // deactivating the technician later does not change the record.
    public string TechnicianName { get; private set; } = string.Empty;

    // One of the refrigerant types in RefrigerantSettings. The entity does not check that, because the types are stored
    // settings.
    public string RefrigerantType { get; private set; } = string.Empty;

    public decimal AmountLbs { get; private set; }

    public string Notes { get; private set; } = string.Empty;

    // The customer's name as it was when the entry was logged, for the same reason as TechnicianName.
    // TicketNumber, JobName, CustomerName and VendorInvoiceRef are null when none was provided.
    public string? TicketNumber { get; private set; }

    public string? JobName { get; private set; }

    public string? CustomerName { get; private set; }

    public string? VendorInvoiceRef { get; private set; }

    public static RefrigerantLogEntry Create(
        DateOnly date,
        string technicianName,
        string refrigerantType,
        decimal amountLbs,
        string notes,
        string? ticketNumber = null,
        string? jobName = null,
        string? customerName = null,
        string? vendorInvoiceRef = null)
    {
        var entry = new RefrigerantLogEntry();
        entry.Update(date, technicianName, refrigerantType, amountLbs, notes, ticketNumber, jobName, customerName, vendorInvoiceRef);

        return entry;
    }

    /// <summary>
    /// Replaces every detail. A blank ticket number, job name, customer name or vendor invoice ref clears it.
    /// </summary>
    public void Update(
        DateOnly date,
        string technicianName,
        string refrigerantType,
        decimal amountLbs,
        string notes,
        string? ticketNumber,
        string? jobName,
        string? customerName,
        string? vendorInvoiceRef)
    {
        if (string.IsNullOrWhiteSpace(technicianName))
        {
            throw new ArgumentException("Technician is required.");
        }

        if (string.IsNullOrWhiteSpace(refrigerantType))
        {
            throw new ArgumentException("Refrigerant type is required.");
        }

        if (amountLbs <= 0)
        {
            throw new ArgumentException("Amount used must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(notes))
        {
            throw new ArgumentException("Notes are required.");
        }

        Date = date;
        TechnicianName = technicianName.Trim();
        RefrigerantType = refrigerantType.Trim();
        AmountLbs = amountLbs;
        Notes = notes.Trim();
        TicketNumber = NullIfBlank(ticketNumber);
        JobName = NullIfBlank(jobName);
        CustomerName = NullIfBlank(customerName);
        VendorInvoiceRef = NullIfBlank(vendorInvoiceRef);
    }

    private static string? NullIfBlank(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
