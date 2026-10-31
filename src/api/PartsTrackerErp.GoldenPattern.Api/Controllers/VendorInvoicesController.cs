using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

namespace PartsTrackerErp.GoldenPattern.Api.Controllers;

[Route("[controller]")]
public class VendorInvoicesController : ApiControllerBase
{
    [HttpPost]
    public Task<IActionResult> Create(
        [FromBody] VendorInvoiceDto invoice,
        IValidator<VendorInvoiceDto> validator,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            invoice,
            validator,
            (request, _) => Task.FromResult(ServiceResponse<VendorInvoiceDto>.Succeed(request)),
            cancellationToken);
}
