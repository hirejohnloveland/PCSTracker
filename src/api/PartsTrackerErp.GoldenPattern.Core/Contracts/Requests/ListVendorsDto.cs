namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

public sealed class ListVendorsDto : PagedRequestDto
{
    /// <summary>
    /// Lists only vendors whose name contains every word of this text, ignoring case. A word matches inside the words of
    /// the name, so "sup" finds "Acme Supply". Null or blank lists every vendor.
    /// </summary>
    public string? Name { get; set; }
}
