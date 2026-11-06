using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;

namespace PartsTrackerErp.GoldenPattern.Api.Controllers;

[Route("[controller]")]
public class QuickbooksController(IQuickbooksApplicationService quickbooksApplicationService) : ApiControllerBase
{
    [HttpPost("vendors/sync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<QuickbooksSyncResponseDto>>> SyncVendors(CancellationToken cancellationToken)
    {
        ServiceResponse<QuickbooksSyncResponseDto> response = await quickbooksApplicationService.SyncVendorsAsync(cancellationToken);
        return ToResult(response);
    }

    [HttpPost("customers/sync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<QuickbooksSyncResponseDto>>> SyncCustomers(CancellationToken cancellationToken)
    {
        ServiceResponse<QuickbooksSyncResponseDto> response = await quickbooksApplicationService.SyncCustomersAsync(cancellationToken);
        return ToResult(response);
    }

    [HttpPost("technicians/sync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<QuickbooksSyncResponseDto>>> SyncTechnicians(CancellationToken cancellationToken)
    {
        ServiceResponse<QuickbooksSyncResponseDto> response = await quickbooksApplicationService.SyncTechniciansAsync(cancellationToken);
        return ToResult(response);
    }
}
