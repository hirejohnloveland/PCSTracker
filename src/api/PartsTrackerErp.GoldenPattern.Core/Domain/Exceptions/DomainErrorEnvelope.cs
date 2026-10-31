namespace PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

/// <summary>
/// The errors of one failure, with the category that decides how the API reports them.
/// </summary>
public sealed record DomainErrorEnvelope
{
    public required ErrorType ErrorType { get; init; }

    public required IReadOnlyList<DomainError> Errors { get; init; }
}
