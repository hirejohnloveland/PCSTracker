namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// The refrigerant types a log entry can use, as configured in the refrigerant settings.
/// </summary>
public sealed record RefrigerantTypesResponseDto(IReadOnlyList<string> Refrigerants);
