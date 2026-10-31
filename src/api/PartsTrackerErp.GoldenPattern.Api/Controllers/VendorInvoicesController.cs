using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Mapping;

namespace PartsTrackerErp.GoldenPattern.Api.Controllers;

[Route("[controller]")]
public class VendorInvoicesController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] VendorInvoiceDto invoice,
        IValidator<VendorInvoiceDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(invoice, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        return ToResult(ServiceResponse<VendorInvoiceResponseDto>.Succeed(invoice.ToResponse()));
    }
}
