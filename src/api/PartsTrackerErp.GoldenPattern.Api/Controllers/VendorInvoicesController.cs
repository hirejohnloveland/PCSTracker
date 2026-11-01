using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Api.Controllers;

[Route("[controller]")]
public class VendorInvoicesController(IVendorInvoicesApplicationService vendorInvoicesApplicationService) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<VendorInvoiceResponseDto>>> Create(
        [FromBody] VendorInvoiceDto invoice,
        IValidator<VendorInvoiceDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(invoice, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<VendorInvoiceResponseDto> response = await vendorInvoicesApplicationService.CreateAsync(invoice, cancellationToken);
        return ToCreatedResult(response);
    }
}
