using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Api.Controllers;

[Route("[controller]")]
public class CustomersController(ICustomersApplicationService customersApplicationService) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> Add(
        [FromBody] CustomerDto request,
        IValidator<CustomerDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<CustomerResponseDto> response = await customersApplicationService.AddAsync(request, cancellationToken);
        return ToCreatedResult(response);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<CustomerResponseDto>>> List(
        [FromQuery] ListCustomersDto request,
        IValidator<ListCustomersDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<PagedResponseDto<CustomerResponseDto>> response =
            await customersApplicationService.ListAsync(request, cancellationToken);
        return ToPagedResult(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> Get(string id, CancellationToken cancellationToken)
    {
        ServiceResponse<CustomerResponseDto> response = await customersApplicationService.GetAsync(id, cancellationToken);
        return ToResult(response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> Update(
        string id,
        [FromBody] CustomerDto request,
        IValidator<CustomerDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<CustomerResponseDto> response = await customersApplicationService.UpdateAsync(id, request, cancellationToken);
        return ToResult(response);
    }
}
