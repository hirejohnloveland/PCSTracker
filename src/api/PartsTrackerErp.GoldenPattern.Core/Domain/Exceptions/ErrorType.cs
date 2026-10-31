namespace PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

/// <summary>
/// The category of a domain failure. The API maps it to an HTTP status code.
/// </summary>
public enum ErrorType
{
    Validation = 0,
    NotFound = 1,
    Unauthorized = 2,
    BusinessRuleViolation = 3,
    Unexpected = 4,
    Forbidden = 5
}
