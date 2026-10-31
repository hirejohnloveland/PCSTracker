using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Api.ErrorHandling;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;
using PartsTrackerErp.GoldenPattern.Core.Utilities;

namespace PartsTrackerErp.GoldenPattern.Api.Controllers;

/// <summary>
/// The standard block every controller inherits. A success becomes an <see cref="ApiResponse{T}"/>.
/// A failure becomes an <c>application/problem+json</c> response.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Validates the request, returning a 400 problem if it is invalid, then calls the use case and answers
    /// with its result.
    /// </summary>
    protected async Task<IActionResult> ExecuteAsync<TRequest, TData>(
        TRequest request,
        IValidator<TRequest> validator,
        Func<TRequest, CancellationToken, Task<ServiceResponse<TData>>> useCase,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ProblemDetailsMapper.ToProblemDetails(validation.Errors).ToObjectResult(HttpContext);
        }

        return ToResult(await useCase(request, cancellationToken));
    }

    protected IActionResult ToResult<T>(ServiceResponse<T> response)
    {
        if (response.Success)
        {
            return Ok(new ApiResponse<T>(
                response.Data,
                new ApiResponseMetadata(ProblemDetailsMapper.GetTraceId(HttpContext), DateTime.UtcNow)));
        }

        // Null or unknown failure types fall through to 500 in ErrorTypeExtensions.
        var envelope = new DomainErrorEnvelope
        {
            ErrorType = response.FailureType ?? ErrorType.Unexpected,
            Errors = response.Errors
        };
        ProblemDetails problem = ProblemDetailsMapper.ToProblemDetails(envelope);

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            // There is no exception for the framework to log.
            HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GetType()).LogError(
                "A use case failed with {ErrorType}: {Errors} {Method} {Path} traceId={TraceId}",
                envelope.ErrorType,
                string.Join(" | ", envelope.Errors.Select(error => $"[{error.Code}] {error.Message}")).SanitizeForLogging(),
                Request.Method.SanitizeForLogging(),
                Request.Path.Value.SanitizeForLogging(),
                ProblemDetailsMapper.GetTraceId(HttpContext));
        }

        return problem.ToObjectResult(HttpContext);
    }
}
