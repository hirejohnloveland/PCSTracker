namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

public sealed class VendorInvoiceDto
{
    public required string VendorName { get; set; }

    public required decimal Balance { get; set; }
}
