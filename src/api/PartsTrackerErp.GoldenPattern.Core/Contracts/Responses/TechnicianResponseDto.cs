namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// A technician as the API reports it. <paramref name="QuickbooksId"/> is null for a technician that is not linked to
/// QuickBooks. <paramref name="Phone"/> and <paramref name="Notes"/> are null when none was provided.
/// </summary>
public sealed record TechnicianResponseDto(
    string Id,
    string FullName,
    string Role,
    bool Active,
    string? QuickbooksId,
    string? Phone,
    string? Notes);
