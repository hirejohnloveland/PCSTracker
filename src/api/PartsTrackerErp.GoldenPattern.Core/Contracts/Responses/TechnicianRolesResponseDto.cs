namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// The roles a technician can have, as configured in the technician role settings.
/// </summary>
public sealed record TechnicianRolesResponseDto(IReadOnlyList<string> Roles);
