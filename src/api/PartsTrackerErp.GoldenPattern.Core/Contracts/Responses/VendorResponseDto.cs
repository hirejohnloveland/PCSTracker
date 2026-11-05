namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// A vendor as the API reports it. <paramref name="QuickbooksId"/> is null for a vendor that is not linked to QuickBooks.
/// </summary>
public sealed record VendorResponseDto(string Id, string Name, bool IsActive, string? QuickbooksId);
