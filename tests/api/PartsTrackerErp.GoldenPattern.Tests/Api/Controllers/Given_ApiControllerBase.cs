using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PartsTrackerErp.GoldenPattern.Api.Controllers;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;
using PartsTrackerErp.GoldenPattern.Tests.Api.ErrorHandling;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.Controllers;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_ApiControllerBase : IDisposable
{
    private static readonly Dictionary<ErrorType, int> ExpectedStatusCodes = new()
    {
        [ErrorType.Validation] = StatusCodes.Status400BadRequest,
        [ErrorType.Unauthorized] = StatusCodes.Status401Unauthorized,
        [ErrorType.Forbidden] = StatusCodes.Status403Forbidden,
        [ErrorType.NotFound] = StatusCodes.Status404NotFound,
        [ErrorType.BusinessRuleViolation] = StatusCodes.Status409Conflict,
        [ErrorType.Unexpected] = StatusCodes.Status500InternalServerError
    };

    private readonly CapturingLoggerProvider logs = new();
    private readonly ServiceProvider services;

    public Given_ApiControllerBase()
    {
        services = ErrorHandlingTestServices.Build(logs);
    }

    public void Dispose() => services.Dispose();

    public static TheoryData<ErrorType, int> StatusCodeCases()
    {
        var cases = new TheoryData<ErrorType, int>();
        foreach ((ErrorType errorType, int statusCode) in ExpectedStatusCodes)
        {
            cases.Add(errorType, statusCode);
        }

        return cases;
    }

    private static DomainError Error(string code, string message) => new() { Code = code, Message = message };

    private TestController NewController(string path = "/vendors")
    {
        DefaultHttpContext httpContext = ErrorHandlingTestServices.NewHttpContext(services, path);
        httpContext.TraceIdentifier = "trace-123";

        return new TestController { ControllerContext = new ControllerContext { HttpContext = httpContext } };
    }

    private static ProblemDetails ProblemOf(IActionResult result) =>
        Assert.IsType<ProblemDetails>(Assert.IsType<ObjectResult>(result).Value);

    private static ProblemError[] ErrorsOf(ProblemDetails problem) =>
        Assert.IsType<ProblemError[]>(problem.Extensions["errors"]);

    // ---- ExecuteAsync ----

    [Fact]
    public async Task ExecuteAsync_InvalidRequest_Returns400WithEntriesAndSkipsTheUseCase()
    {
        bool called = false;

        IActionResult result = await NewController().Run(
            new NameRequest { Name = string.Empty },
            new NameRequestValidator(),
            (_, _) =>
            {
                called = true;
                return Task.FromResult(ServiceResponse<string>.Succeed("done"));
            },
            CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal(["application/problem+json"], objectResult.ContentTypes);
        ProblemDetails problem = ProblemOf(result);
        Assert.Equal("Validation Failed", problem.Title);
        ProblemError error = Assert.Single(ErrorsOf(problem));
        Assert.Equal("NotEmptyValidator", error.Code);
        Assert.Equal("Name is required.", error.Detail);
        Assert.Equal("#/name", error.Pointer);
        Assert.False(called);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_CallsTheUseCaseWithTheRequestAndTokenAndAnswersWithItsData()
    {
        using var cancellation = new CancellationTokenSource();
        var request = new NameRequest { Name = "Acme" };
        NameRequest? seenRequest = null;
        CancellationToken seenToken = default;

        IActionResult result = await NewController().Run(
            request,
            new NameRequestValidator(),
            (r, token) =>
            {
                seenRequest = r;
                seenToken = token;
                return Task.FromResult(ServiceResponse<string>.Succeed("done"));
            },
            cancellation.Token);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("done", Assert.IsType<ApiResponse<string>>(ok.Value).Data);
        Assert.Same(request, seenRequest);
        Assert.Equal(cancellation.Token, seenToken);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_AFailedUseCaseBecomesAProblem()
    {
        IActionResult result = await NewController().Run(
            new NameRequest { Name = "Acme" },
            new NameRequestValidator(),
            (_, _) => Task.FromResult(ServiceResponse<string>.Fail(ErrorType.NotFound, Error("VendorNotFound", "Vendor 7 was not found."))),
            CancellationToken.None);

        Assert.Equal(StatusCodes.Status404NotFound, Assert.IsType<ObjectResult>(result).StatusCode);
        Assert.Equal("VendorNotFound", Assert.Single(ErrorsOf(ProblemOf(result))).Code);
    }

    // ---- ToResult: success ----

    [Fact]
    public void ToResult_Success_ReturnsOkWithTheDataAndMetadata()
    {
        Assert.Null(Activity.Current);
        DateTime before = DateTime.UtcNow;

        IActionResult result = NewController().Respond(ServiceResponse<string>.Succeed("Acme"));

        DateTime after = DateTime.UtcNow;
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        var response = Assert.IsType<ApiResponse<string>>(ok.Value);
        Assert.Equal("Acme", response.Data);
        Assert.Equal("trace-123", response.Metadata.TraceId);
        Assert.Equal(DateTimeKind.Utc, response.Metadata.Timestamp.Kind);
        Assert.InRange(response.Metadata.Timestamp, before, after);
    }

    [Fact]
    public void ToResult_Success_TraceIdIsTheActivityIdWhenThereIsOne()
    {
        using var activity = new Activity("test").Start();

        IActionResult result = NewController().Respond(ServiceResponse<string>.Succeed("Acme"));

        var response = Assert.IsType<ApiResponse<string>>(Assert.IsType<OkObjectResult>(result).Value);
        Assert.Equal(activity.Id, response.Metadata.TraceId);
    }

    // ---- ToResult: failure ----

    [Theory]
    [MemberData(nameof(StatusCodeCases))]
    public void ToResult_Failure_MapsEachErrorTypeToItsStatus(ErrorType errorType, int expectedStatus)
    {
        IActionResult result = NewController().Respond(ServiceResponse<string>.Fail(errorType, Error("Code", "Message.")));

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(expectedStatus, objectResult.StatusCode);
        Assert.Equal(expectedStatus, ProblemOf(result).Status);
        Assert.Equal(["application/problem+json"], objectResult.ContentTypes);
    }

    [Fact]
    public void ToResult_Failure_EveryErrorTypeIsCoveredByTheseTests()
    {
        // Fails when an ErrorType is added without a decision about its status code.
        Assert.Equal(
            Enum.GetValues<ErrorType>().OrderBy(errorType => errorType),
            ExpectedStatusCodes.Keys.OrderBy(errorType => errorType));
    }

    [Fact]
    public void ToResult_Failure_UndefinedErrorType_Gives500()
    {
        IActionResult result = NewController().Respond(ServiceResponse<string>.Fail((ErrorType)999, Error("Code", "Message.")));

        Assert.Equal(StatusCodes.Status500InternalServerError, Assert.IsType<ObjectResult>(result).StatusCode);
    }

    [Fact]
    public void ToResult_Failure_BuildsTheProblemThroughTheFrameworkFactory()
    {
        IActionResult result = NewController("/vendors/7/activate").Respond(
            ServiceResponse<string>.Fail(ErrorType.NotFound, Error("VendorNotFound", "Vendor 7 was not found.")));

        ProblemDetails problem = ProblemOf(result);
        Assert.Equal("Not Found", problem.Title);
        Assert.Equal("https://api.example.com/problems/not-found", problem.Type);
        Assert.Equal("Vendor 7 was not found.", problem.Detail);
        Assert.Equal("/vendors/7/activate", problem.Instance);
        Assert.Equal("trace-123", problem.Extensions["traceId"]);
        Assert.Equal(
            [new ProblemError("VendorNotFound", "Vendor 7 was not found.", null)],
            ErrorsOf(problem));
    }

    // ---- ToResult: logging ----

    [Fact]
    public void ToResult_ServerFailure_LogsErrorWithTheCodeAndMessage()
    {
        NewController().Respond(ServiceResponse<string>.Fail(ErrorType.Unexpected, Error("Boom", "It broke.")));

        LogEntry entry = Assert.Single(logs.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains("[Boom] It broke.", entry.Message);
        Assert.Null(entry.Exception);
    }

    [Fact]
    public void ToResult_ServerFailure_LogsOnOneLineWhateverTheCallerSent()
    {
        NewController("/vendors/1\r\nFAKE LOG LINE").Respond(
            ServiceResponse<string>.Fail(ErrorType.Unexpected, Error("Boom", "It broke.\r\nFAKE MESSAGE LINE")));

        LogEntry entry = Assert.Single(logs.Entries);
        Assert.DoesNotContain('\n', entry.Message);
        Assert.DoesNotContain('\r', entry.Message);
        Assert.Contains("/vendors/1 FAKE LOG LINE", entry.Message);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.BusinessRuleViolation)]
    public void ToResult_ClientFailure_LogsNothing(ErrorType errorType)
    {
        NewController().Respond(ServiceResponse<string>.Fail(errorType, Error("Code", "Message.")));

        Assert.Empty(logs.Entries);
    }

    private sealed class TestController : ApiControllerBase
    {
        public Task<IActionResult> Run<TRequest, TData>(
            TRequest request,
            IValidator<TRequest> validator,
            Func<TRequest, CancellationToken, Task<ServiceResponse<TData>>> useCase,
            CancellationToken cancellationToken) =>
            ExecuteAsync(request, validator, useCase, cancellationToken);

        public IActionResult Respond<T>(ServiceResponse<T> response) => ToResult(response);
    }

    private sealed class NameRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class NameRequestValidator : AbstractValidator<NameRequest>
    {
        public NameRequestValidator()
        {
            RuleFor(request => request.Name).NotEmpty().WithMessage("Name is required.");
        }
    }
}
