using FluentValidation.Results;
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
    /// Answers a request that failed validation with a 400 problem that lists each failure.
    /// </summary>
    protected IActionResult InvalidRequest(ValidationResult validation)
    {
        return ProblemDetailsMapper.ToProblemDetails(validation.Errors).ToObjectResult(HttpContext);
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
