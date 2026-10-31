using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

namespace PartsTrackerErp.GoldenPattern.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class VendorInvoicesController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] VendorInvoiceDto invoice,
        IValidator<VendorInvoiceDto> validator,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(invoice, cancellationToken);
        if (!result.IsValid)
        {
            return BadRequest(result.Errors);
        }

        return Ok(invoice);
    }
}
