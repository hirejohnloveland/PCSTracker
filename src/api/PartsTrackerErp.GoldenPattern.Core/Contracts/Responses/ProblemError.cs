using System.Text.Json.Serialization;

namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

/// <summary>
/// One entry in the <c>errors</c> array of a problem response.
/// </summary>
/// <param name="Code">Stable, machine-readable identifier of the failure.</param>
/// <param name="Detail">What went wrong and, where possible, what to change.</param>
/// <param name="Pointer">
/// Where in the request the fault is: a JSON Pointer to a body member, such as <c>#/name</c>, or <c>#/</c> plus a query
/// parameter name, such as <c>#/pageSize</c>. Only request problems have one.
/// </param>
public sealed record ProblemError(
    string Code,
    string Detail,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Pointer);
