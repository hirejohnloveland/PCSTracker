using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Api.Controllers;

[Route("[controller]")]
public class TechniciansController(ITechniciansApplicationService techniciansApplicationService) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<TechnicianResponseDto>>> Add(
        [FromBody] TechnicianDto request,
        IValidator<TechnicianDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<TechnicianResponseDto> response = await techniciansApplicationService.AddAsync(request, cancellationToken);
        return ToCreatedResult(response);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<TechnicianResponseDto>>> List(
        [FromQuery] ListTechniciansDto request,
        IValidator<ListTechniciansDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<PagedResponseDto<TechnicianResponseDto>> response =
            await techniciansApplicationService.ListAsync(request, cancellationToken);
        return ToPagedResult(response);
    }

    [HttpGet("roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianRolesResponseDto>>> GetRoles(CancellationToken cancellationToken)
    {
        ServiceResponse<TechnicianRolesResponseDto> response = await techniciansApplicationService.GetRolesAsync(cancellationToken);
        return ToResult(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianResponseDto>>> Get(string id, CancellationToken cancellationToken)
    {
        ServiceResponse<TechnicianResponseDto> response = await techniciansApplicationService.GetAsync(id, cancellationToken);
        return ToResult(response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TechnicianResponseDto>>> Update(
        string id,
        [FromBody] TechnicianDto request,
        IValidator<TechnicianDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<TechnicianResponseDto> response = await techniciansApplicationService.UpdateAsync(id, request, cancellationToken);
        return ToResult(response);
    }
}
