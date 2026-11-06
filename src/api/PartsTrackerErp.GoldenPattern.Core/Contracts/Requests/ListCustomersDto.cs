namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

public sealed class ListCustomersDto : PagedRequestDto
{
    /// <summary>
    /// Lists only customers whose name contains every word of this text, ignoring case. A word matches inside the words of
    /// the name, so "sup" finds "Acme Supply". Null or blank lists every customer.
    /// </summary>
    public string? Name { get; set; }
}
