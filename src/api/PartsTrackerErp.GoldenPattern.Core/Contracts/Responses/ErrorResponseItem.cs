namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// One failure in an <see cref="ErrorResponse"/>.
/// </summary>
/// <param name="Code">Stable, machine-readable identifier of the failure.</param>
/// <param name="Message">Human-readable description.</param>
public sealed record ErrorResponseItem(string Code, string Message);
