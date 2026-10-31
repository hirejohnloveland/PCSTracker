using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.ApplicationServices.Implementations;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_VendorInvoicesApplicationService
{
    private readonly VendorInvoicesApplicationService _service = new();

    [Fact]
    public async Task CreateAsync_SucceedsWithTheInvoice()
    {
        var invoice = new VendorInvoiceDto { VendorName = "Acme", Balance = 10.5m };

        ServiceResponse<VendorInvoiceResponseDto> response = await _service.CreateAsync(invoice);

        Assert.True(response.Success);
        Assert.Equal(new VendorInvoiceResponseDto("Acme", 10.5m), response.Data);
    }
}
