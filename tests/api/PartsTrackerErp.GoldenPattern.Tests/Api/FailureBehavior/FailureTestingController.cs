using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartsTrackerErp.GoldenPattern.Api.Controllers;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.FailureBehavior;

/// <summary>
/// A controller built the way production controllers are, whose outcome the test sets through
/// <see cref="FailureTestingService"/>.
/// </summary>
[Route("testing/failures")]
public sealed class FailureTestingController(FailureTestingService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Run(CancellationToken cancellationToken)
    {
        ServiceResponse<string> response = await service.RunAsync(cancellationToken);
        return ToResult(response);
    }

    [HttpPost]
    public async Task<IActionResult> Validate(
        [FromBody] FailureTestingRequest request,
        IValidator<FailureTestingRequest> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return InvalidRequest(validation);
        }

        ServiceResponse<string> response = await service.RunAsync(cancellationToken);
        return ToResult(response);
    }

    [Authorize]
    [HttpGet("secured")]
    public IActionResult Secured()
    {
        return Ok();
    }
}

/// <summary>
/// An endpoint that is not an <c>[ApiController]</c>, so MVC's problem writer does not take it over.
/// </summary>
[Route("testing/plain")]
public sealed class PlainFailureTestingController(FailureTestingService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Run(CancellationToken cancellationToken)
    {
        await service.RunAsync(cancellationToken);
        return Ok();
    }
}
