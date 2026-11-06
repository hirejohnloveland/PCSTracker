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
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<VendorResponseDto>>> Add(
        [FromBody] VendorDto request,
        IValidator<VendorDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<VendorResponseDto> response = await vendorsApplicationService.AddAsync(request, cancellationToken);
        return ToCreatedResult(response);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<VendorResponseDto>>> List(
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<VendorResponseDto>>> Get(string id, CancellationToken cancellationToken)
    {
        ServiceResponse<VendorResponseDto> response = await vendorsApplicationService.GetAsync(id, cancellationToken);
        return ToResult(response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<VendorResponseDto>>> Update(
        string id,
        [FromBody] VendorDto request,
        IValidator<VendorDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<VendorResponseDto> response = await vendorsApplicationService.UpdateAsync(id, request, cancellationToken);
        return ToResult(response);
    }
}
