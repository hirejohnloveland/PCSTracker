namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

public sealed class VendorInvoiceDto
{
    public string VendorName { get; set; } = string.Empty;

    public decimal Balance { get; set; }
}
