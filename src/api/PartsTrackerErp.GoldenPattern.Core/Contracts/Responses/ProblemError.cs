using System.Text.Json.Serialization;

namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// One entry in the <c>errors</c> array of a problem response.
/// </summary>
/// <param name="Code">Stable, machine-readable identifier of the failure.</param>
/// <param name="Detail">What went wrong and, where possible, what to change.</param>
/// <param name="Pointer">JSON Pointer to the part of the request body at fault. Only request problems have one.</param>
public sealed record ProblemError(
    string Code,
    string Detail,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Pointer);
