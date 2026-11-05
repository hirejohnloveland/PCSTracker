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
}
