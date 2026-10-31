namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// A vendor invoice as the API reports it.
/// </summary>
public sealed record VendorInvoiceResponseDto(string VendorName, decimal Balance);
