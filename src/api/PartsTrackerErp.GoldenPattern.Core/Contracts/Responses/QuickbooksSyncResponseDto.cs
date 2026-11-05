namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// The outcome of a vendor sync with QuickBooks.
/// </summary>
public sealed record QuickbooksSyncResponseDto(int VendorsSynced);
