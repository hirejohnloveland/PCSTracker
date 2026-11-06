using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Core.Mapping;

public static class RefrigerantLogEntryMappingExtensions
{
    public static RefrigerantLogEntryResponseDto ToResponseDto(this RefrigerantLogEntry entry)
    {
        string id = entry.Id
            ?? throw new InvalidOperationException("A refrigerant log entry has no id until it is stored, and a response describes a stored entry.");

        return new RefrigerantLogEntryResponseDto(
            Id: id,
            Date: entry.Date,
            Technician: entry.TechnicianName,
            RefrigerantType: entry.RefrigerantType,
            AmountLbs: entry.AmountLbs,
            Notes: entry.Notes,
            TicketNumber: entry.TicketNumber,
            JobName: entry.JobName,
            Customer: entry.CustomerName,
            VendorInvoiceRef: entry.VendorInvoiceRef);
    }
}
