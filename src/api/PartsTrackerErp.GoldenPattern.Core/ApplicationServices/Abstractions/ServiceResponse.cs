using System.Diagnostics.CodeAnalysis;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;

/// <summary>
/// The outcome of an application service call: the data, or a failure the caller must be told about.
/// Unexpected failures are exceptions, not responses. Create one with <see cref="Succeed"/> or <see cref="Fail(ErrorType, DomainError[])"/>.
/// </summary>
public sealed class ServiceResponse<T>
{
    private ServiceResponse(bool success, T? data, ErrorType? failureType, IReadOnlyList<DomainError> errors)
    {
        Success = success;
        Data = data;
        FailureType = failureType;
        Errors = errors;
    }

    [MemberNotNullWhen(true, nameof(Data))]
    public bool Success { get; }

    public T? Data { get; }

    /// <summary>
    /// The category of the failure, as given to <c>Fail</c>. Null when the call succeeded.
    /// </summary>
    public ErrorType? FailureType { get; }

    public IReadOnlyList<DomainError> Errors { get; }

    public static ServiceResponse<T> Succeed(T data)
    {
        return new(true, data, null, []);
    }

    public static ServiceResponse<T> Fail(ErrorType type, params DomainError[] errors)
    {
        return new(false, default, type, [.. errors]);
    }

    public static ServiceResponse<T> Fail(DomainErrorEnvelope envelope)
    {
        return new(false, default, envelope.ErrorType, [.. envelope.Errors]);
    }
}
