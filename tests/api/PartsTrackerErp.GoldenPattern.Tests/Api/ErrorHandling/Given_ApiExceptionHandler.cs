using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PartsTrackerErp.GoldenPattern.Api.ErrorHandling;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.ErrorHandling;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_ApiExceptionHandler : IDisposable
{
    private readonly CapturingLoggerProvider logs = new();
    private readonly ServiceProvider services;
    private readonly ApiExceptionHandler handler;

    public Given_ApiExceptionHandler()
    {
        services = ErrorHandlingTestServices.Build(logs);
        handler = new ApiExceptionHandler(
            services.GetRequiredService<IProblemDetailsService>(),
            services.GetRequiredService<ILogger<ApiExceptionHandler>>());
    }

    public void Dispose() => services.Dispose();

    private static DomainError Error(string code, string message) => new() { Code = code, Message = message };

    private async Task<(bool Handled, DefaultHttpContext Context)> HandleAsync(
        Exception exception,
        Action<DefaultHttpContext>? configure = null)
    {
        DefaultHttpContext context = ErrorHandlingTestServices.NewHttpContext(services);
        configure?.Invoke(context);

        bool handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        return (handled, context);
    }

    private static JsonDocument ReadProblem(HttpContext context) =>
        JsonDocument.Parse(ErrorHandlingTestServices.ReadBody(context));

    private static string[] Codes(JsonElement problem) =>
        [.. problem.GetProperty("errors").EnumerateArray().Select(error => error.GetProperty("code").GetString()!)];

    [Fact]
    public async Task TryHandleAsync_DomainException_WritesTheProblemForItsCategory()
    {
        var exception = new DomainException(
            ErrorType.NotFound,
            Error("VendorNotFound", "Vendor 7 was not found."));

        (bool handled, DefaultHttpContext context) = await HandleAsync(
            exception,
            c =>
            {
                c.TraceIdentifier = "trace-123";
                c.Request.Path = "/vendors/7/activate";
            });

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);
        using JsonDocument body = ReadProblem(context);
        JsonElement problem = body.RootElement;
        Assert.Equal(404, problem.GetProperty("status").GetInt32());
        Assert.Equal("https://api.example.com/problems/not-found", problem.GetProperty("type").GetString());
        Assert.Equal("Vendor 7 was not found.", problem.GetProperty("detail").GetString());
        Assert.Equal("/vendors/7/activate", problem.GetProperty("instance").GetString());
        Assert.Equal("trace-123", problem.GetProperty("traceId").GetString());
        JsonElement error = Assert.Single(problem.GetProperty("errors").EnumerateArray());
        Assert.Equal("VendorNotFound", error.GetProperty("code").GetString());
        Assert.Equal("Vendor 7 was not found.", error.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_DomainExceptionWithSeveralErrors_WritesEveryErrorInOrder()
    {
        var exception = new DomainException(
            ErrorType.Validation,
            Error("First", "First problem."),
            Error("Second", "Second problem."));

        (_, DefaultHttpContext context) = await HandleAsync(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        using JsonDocument body = ReadProblem(context);
        Assert.Equal(["First", "Second"], Codes(body.RootElement));
        Assert.Equal("One or more validation errors occurred.", body.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_HttpRequestException_Writes502WithAFixedMessage()
    {
        var exception = new HttpRequestException("Upstream said no.", null, HttpStatusCode.ServiceUnavailable);

        (bool handled, DefaultHttpContext context) = await HandleAsync(exception);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status502BadGateway, context.Response.StatusCode);
        using JsonDocument body = ReadProblem(context);
        Assert.Equal(["UpstreamServiceUnavailable"], Codes(body.RootElement));
        Assert.DoesNotContain("Upstream said no.", body.RootElement.GetRawText());
    }

    [Fact]
    public async Task TryHandleAsync_HttpClientTimeout_Writes504()
    {
        var exception = new TaskCanceledException("The request timed out.", new TimeoutException());

        (bool handled, DefaultHttpContext context) = await HandleAsync(exception);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status504GatewayTimeout, context.Response.StatusCode);
        using JsonDocument body = ReadProblem(context);
        Assert.Equal(["UpstreamServiceTimeout"], Codes(body.RootElement));
    }

    [Theory]
    [InlineData(StatusCodes.Status408RequestTimeout)]
    [InlineData(StatusCodes.Status413PayloadTooLarge)]
    public async Task TryHandleAsync_BadHttpRequest_KeepsItsOwnStatusAndLeaksNoExceptionText(int status)
    {
        var exception = new BadHttpRequestException("Server=secret;Password=hunter2.", status);

        (bool handled, DefaultHttpContext context) = await HandleAsync(exception);

        Assert.True(handled);
        Assert.Equal(status, context.Response.StatusCode);
        using JsonDocument body = ReadProblem(context);
        Assert.Equal(status, body.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(["InvalidRequest"], Codes(body.RootElement));
        Assert.DoesNotContain("hunter2", body.RootElement.GetRawText());
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_ReturnsFalseAndLeavesTheResponseToTheFramework()
    {
        (bool handled, DefaultHttpContext context) = await HandleAsync(new InvalidOperationException("Boom."));

        Assert.False(handled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
        Assert.Empty(logs.Entries);
    }

    [Fact]
    public async Task TryHandleAsync_CancellationNotCausedByATimeout_ReturnsFalse()
    {
        (bool handled, _) = await HandleAsync(new TaskCanceledException("Cancelled."));

        Assert.False(handled);
    }

    [Fact]
    public async Task TryHandleAsync_ClientErrorDomainException_LogsWarningWithTheException()
    {
        var exception = new DomainException(ErrorType.BusinessRuleViolation, Error("VendorInactive", "Vendor 7 is inactive."));

        await HandleAsync(exception);

        LogEntry entry = Assert.Single(logs.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Same(exception, entry.Exception);
    }

    [Fact]
    public async Task TryHandleAsync_ServerErrorDomainException_LogsError()
    {
        var exception = new DomainException(ErrorType.Unexpected, Error("Boom", "It broke."));

        await HandleAsync(exception);

        LogEntry entry = Assert.Single(logs.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Same(exception, entry.Exception);
    }

    [Fact]
    public async Task TryHandleAsync_UpstreamFailures_LogWarning()
    {
        await HandleAsync(new HttpRequestException("Could not connect."));
        await HandleAsync(new TaskCanceledException("Timed out.", new TimeoutException()));

        Assert.Equal([LogLevel.Warning, LogLevel.Warning], logs.Entries.Select(entry => entry.Level));
    }

    [Fact]
    public async Task TryHandleAsync_BadHttpRequest_LogsWarning()
    {
        await HandleAsync(new BadHttpRequestException("Too large.", StatusCodes.Status413PayloadTooLarge));

        Assert.Equal(LogLevel.Warning, Assert.Single(logs.Entries).Level);
    }

    [Fact]
    public async Task TryHandleAsync_RequestPathWithLineBreaks_LogsItOnOneLine()
    {
        await HandleAsync(
            new HttpRequestException("Could not connect."),
            c => c.Request.Path = "/vendors/1\r\nFAKE LOG LINE");

        LogEntry entry = Assert.Single(logs.Entries);
        Assert.DoesNotContain('\n', entry.Message);
        Assert.DoesNotContain('\r', entry.Message);
        Assert.Contains("/vendors/1 FAKE LOG LINE", entry.Message);
    }

    [Fact]
    public async Task TryHandleAsync_ClientDoesNotAcceptJson_KeepsTheStatusAndStillCountsAsHandled()
    {
        var exception = new DomainException(ErrorType.NotFound, Error("VendorNotFound", "Vendor 7 was not found."));

        (bool handled, DefaultHttpContext context) = await HandleAsync(
            exception,
            c => c.Request.Headers.Accept = "text/html");

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    [Fact]
    public async Task TryHandleAsync_ApiControllerEndpoint_WritesThroughMvcWithTheAuthoredErrors()
    {
        var exception = new DomainException(ErrorType.NotFound, Error("VendorNotFound", "Vendor 7 was not found."));
        var endpoint = new Endpoint(null, new EndpointMetadataCollection(new ApiControllerAttribute()), "test");

        (bool handled, DefaultHttpContext context) = await HandleAsync(
            exception,
            c =>
            {
                c.TraceIdentifier = "trace-123";
                c.Request.Path = "/vendors/7/activate";

                // MVC's writer only takes over for an [ApiController] endpoint. It writes even when Accept
                // excludes JSON, which tells it apart from the default writer.
                c.Request.Headers.Accept = "text/html";
                c.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature { Endpoint = endpoint });
            });

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);
        using JsonDocument body = ReadProblem(context);
        JsonElement problem = body.RootElement;
        Assert.Equal("https://api.example.com/problems/not-found", problem.GetProperty("type").GetString());
        Assert.Equal("/vendors/7/activate", problem.GetProperty("instance").GetString());
        Assert.Equal("trace-123", problem.GetProperty("traceId").GetString());
        Assert.Equal(["VendorNotFound"], Codes(problem));
    }
}
