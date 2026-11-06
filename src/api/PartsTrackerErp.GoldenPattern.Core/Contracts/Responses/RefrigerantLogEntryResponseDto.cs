namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// A refrigerant log entry as the API reports it. <paramref name="TicketNumber"/>, <paramref name="JobName"/>,
/// <paramref name="Customer"/> and <paramref name="VendorInvoiceRef"/> are null when none was provided.
/// </summary>
public sealed record RefrigerantLogEntryResponseDto(
    string Id,
    DateOnly Date,
    string Technician,
    string RefrigerantType,
    decimal AmountLbs,
    string Notes,
    string? TicketNumber,
    string? JobName,
    string? Customer,
    string? VendorInvoiceRef);
