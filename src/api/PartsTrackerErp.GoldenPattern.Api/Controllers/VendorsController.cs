using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Api.Controllers;

[Route("[controller]")]
public class VendorsController(IVendorsApplicationService vendorsApplicationService) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Add(
        [FromBody] CreateVendorDto request,
        IValidator<CreateVendorDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<VendorResponseDto> response = await vendorsApplicationService.AddAsync(request.Name, cancellationToken);
        return ToCreatedResult(response);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] ListVendorsDto request,
        IValidator<ListVendorsDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<PagedResponseDto<VendorResponseDto>> response =
            await vendorsApplicationService.ListAsync(request, cancellationToken);
        return ToPagedResult(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
    {
        ServiceResponse<VendorResponseDto> response = await vendorsApplicationService.GetAsync(id, cancellationToken);
        return ToResult(response);
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(string id, CancellationToken cancellationToken)
    {
        ServiceResponse<VendorResponseDto> response = await vendorsApplicationService.ActivateAsync(id, cancellationToken);
        return ToResult(response);
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(string id, CancellationToken cancellationToken)
    {
        ServiceResponse<VendorResponseDto> response = await vendorsApplicationService.DeactivateAsync(id, cancellationToken);
        return ToResult(response);
    }
}
