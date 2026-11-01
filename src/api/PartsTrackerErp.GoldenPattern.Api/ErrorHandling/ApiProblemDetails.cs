using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Api.ErrorHandling;

// Describes a failure to the OpenAPI document. Nothing creates one: a failure is sent as a plain ProblemDetails whose
// Extensions carry errors, added by ProblemDetailsMapper, and traceId, added by the framework. A member added to
// either is added here too. Given_OpenApiDocument checks real failures against this type.

/// <summary>
/// A failure response: the problem details of RFC 9457, the trace id of the request, and the errors behind the failure.
/// </summary>
public sealed class ApiProblemDetails : ProblemDetails
{
    /// <summary>
    /// Identifies the request in the server's logs and traces: the W3C trace id when there is one, otherwise the
    /// ASP.NET Core trace identifier.
    /// </summary>
    [JsonPropertyName("traceId")]
    public required string TraceId { get; init; }

    /// <summary>
    /// One entry for each failure found. A response has at least one.
    /// </summary>
    [JsonPropertyName("errors")]
    public required IReadOnlyList<ProblemError> Errors { get; init; }
}
