using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

namespace PartsTrackerErp.GoldenPattern.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class VendorsController(IVendorsApplicationService vendorsApplicationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Add(
        [FromBody] CreateVendorDto request,
        IValidator<CreateVendorDto> validator,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
            return BadRequest(result.Errors);

        var vendor = await vendorsApplicationService.AddAsync(request.Name, cancellationToken);
        return Ok(vendor);
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(string id, CancellationToken cancellationToken)
    {
        var vendor = await vendorsApplicationService.ActivateAsync(id, cancellationToken);
        if (vendor is null)
            return NotFound();

        return Ok(vendor);
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(string id, CancellationToken cancellationToken)
    {
        var vendor = await vendorsApplicationService.DeactivateAsync(id, cancellationToken);
        if (vendor is null)
            return NotFound();

        return Ok(vendor);
    }
}
