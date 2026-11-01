namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

public sealed class ListVendorsDto : PagedRequestDto
{
    /// <summary>
    /// Lists only vendors whose name starts with this text, ignoring case. Null or blank lists every vendor.
    /// </summary>
    public string? Name { get; set; }
}
