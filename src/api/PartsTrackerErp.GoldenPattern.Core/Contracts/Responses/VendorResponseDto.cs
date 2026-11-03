namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// A vendor as the API reports it.
/// </summary>
public sealed record VendorResponseDto(string Id, string Name, bool IsActive);
