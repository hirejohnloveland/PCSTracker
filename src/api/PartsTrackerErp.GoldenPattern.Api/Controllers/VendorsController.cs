using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;

namespace PartsTrackerErp.GoldenPattern.Api.Controllers;

[Route("[controller]")]
public class VendorsController(IVendorsApplicationService vendorsApplicationService) : ApiControllerBase
{
    [HttpPost]
    public Task<IActionResult> Add(
        [FromBody] CreateVendorDto request,
        IValidator<CreateVendorDto> validator,
        CancellationToken cancellationToken) =>
        ExecuteAsync(request, validator, (r, ct) => vendorsApplicationService.AddAsync(r.Name, ct), cancellationToken);

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(string id, CancellationToken cancellationToken) =>
        ToResult(await vendorsApplicationService.ActivateAsync(id, cancellationToken));

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(string id, CancellationToken cancellationToken) =>
        ToResult(await vendorsApplicationService.DeactivateAsync(id, cancellationToken));
}
