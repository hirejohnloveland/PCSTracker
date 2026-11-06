namespace PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

public sealed class CreateVendorDto : IVendorDetailsDto
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? Contact { get; set; }

    public string? Phone { get; set; }

    public string? Notes { get; set; }

    public string? QuickbooksId { get; set; }
}
