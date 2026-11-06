using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Api.Controllers;

[Route("[controller]")]
public class RefrigerantLogController(IRefrigerantLogApplicationService refrigerantLogApplicationService) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<RefrigerantLogEntryResponseDto>>> Add(
        [FromBody] RefrigerantLogEntryDto request,
        IValidator<RefrigerantLogEntryDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<RefrigerantLogEntryResponseDto> response =
            await refrigerantLogApplicationService.AddAsync(request, cancellationToken);
        return ToCreatedResult(response);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedApiResponse<RefrigerantLogEntryResponseDto>>> List(
        [FromQuery] ListRefrigerantLogEntriesDto request,
        IValidator<ListRefrigerantLogEntriesDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<PagedResponseDto<RefrigerantLogEntryResponseDto>> response =
            await refrigerantLogApplicationService.ListAsync(request, cancellationToken);
        return ToPagedResult(response);
    }

    [HttpGet("refrigerants")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RefrigerantTypesResponseDto>>> GetRefrigerantTypes(
        CancellationToken cancellationToken)
    {
        ServiceResponse<RefrigerantTypesResponseDto> response =
            await refrigerantLogApplicationService.GetRefrigerantTypesAsync(cancellationToken);
        return ToResult(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RefrigerantLogEntryResponseDto>>> Get(
        string id,
        CancellationToken cancellationToken)
    {
        ServiceResponse<RefrigerantLogEntryResponseDto> response =
            await refrigerantLogApplicationService.GetAsync(id, cancellationToken);
        return ToResult(response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RefrigerantLogEntryResponseDto>>> Update(
        string id,
        [FromBody] RefrigerantLogEntryDto request,
        IValidator<RefrigerantLogEntryDto> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<RefrigerantLogEntryResponseDto> response =
            await refrigerantLogApplicationService.UpdateAsync(id, request, cancellationToken);
        return ToResult(response);
    }
}
