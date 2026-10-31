namespace PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

/// <summary>
/// Thrown when a use case fails in a way the caller must be told about: a rule was broken, something
/// was not found, access was denied. Throw it directly rather than subclassing it. The
/// <see cref="ErrorType"/> and <see cref="DomainError"/> items carry the detail, and the API's global
/// exception handler turns them into the HTTP response.
/// </summary>
public class DomainException : Exception
{
    public DomainException(DomainErrorEnvelope errorEnvelope, Exception? innerException = null)
        : base(BuildMessage(errorEnvelope), innerException)
    {
        ErrorEnvelope = errorEnvelope;
    }

    public DomainException(ErrorType errorType, params DomainError[] errors)
        : this(new DomainErrorEnvelope { ErrorType = errorType, Errors = [.. errors] })
    {
    }

    public DomainException(ErrorType errorType, IEnumerable<DomainError> errors, Exception? innerException = null)
        : this(new DomainErrorEnvelope { ErrorType = errorType, Errors = [.. errors] }, innerException)
    {
    }

    public DomainErrorEnvelope ErrorEnvelope { get; }

    public IReadOnlyList<DomainError> Errors => ErrorEnvelope.Errors;

    // Exception.Message is for logs only. Callers receive the errors themselves.
    private static string BuildMessage(DomainErrorEnvelope errorEnvelope)
    {
        return errorEnvelope.Errors.Count == 0
            ? "DomainException with no errors."
            : string.Join(" | ", errorEnvelope.Errors.Select(error => $"[{error.Code}] {error.Message}"));
    }
}
