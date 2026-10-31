namespace PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

/// <summary>
/// One failure reported to the caller. <see cref="Code"/> is stable and machine-readable;
/// <see cref="Message"/> is for people.
/// </summary>
public sealed record DomainError
{
    public required string Code { get; init; }

    public required string Message { get; init; }
}
