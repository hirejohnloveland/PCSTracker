using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using PartsTrackerErp.GoldenPattern.Api.ErrorHandling;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.ErrorHandling;

/// <summary>Unit tests for <see cref="GlobalExceptionHandler"/>, exercised through <c>InvokeAsync</c> (status, body, and log).</summary>
[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_GlobalExceptionHandler
{
    private static readonly JsonSerializerOptions WebOptions = new(JsonSerializerDefaults.Web);

    private static DefaultHttpContext NewContext() => new()
    {
        Response = { Body = new MemoryStream() }
    };

    private static async Task<(DefaultHttpContext Context, CapturingLogger Logger)> InvokeAsync(
        Exception exceptionToThrow,
        Action<DefaultHttpContext>? configure = null)
    {
        var logger = new CapturingLogger();
        var handler = new GlobalExceptionHandler(_ => throw exceptionToThrow, logger);
        DefaultHttpContext context = NewContext();
        configure?.Invoke(context);

        await handler.InvokeAsync(context);

        return (context, logger);
    }

    private static string ReadBody(DefaultHttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return new StreamReader(context.Response.Body).ReadToEnd();
    }

    private static ErrorResponse ReadErrorResponse(DefaultHttpContext context) =>
        JsonSerializer.Deserialize<ErrorResponse>(ReadBody(context), WebOptions)!;

    [Fact]
    public async Task InvokeAsync_NoException_LeavesResponseUntouched()
    {
        var handler = new GlobalExceptionHandler(_ => Task.CompletedTask, new CapturingLogger());
        DefaultHttpContext context = NewContext();

        await handler.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    [Fact]
    public async Task InvokeAsync_DomainException_MapsErrorTypeToStatusAndWritesErrorResponse()
    {
        var domainException = new DomainException(
            ErrorType.NotFound,
            new DomainError { Code = "VendorNotFound", Message = "Vendor 7 was not found." });
        DateTimeOffset before = DateTimeOffset.UtcNow;

        (DefaultHttpContext context, _) = await InvokeAsync(domainException, c => c.TraceIdentifier = "trace-123");

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType);
        ErrorResponse body = ReadErrorResponse(context);
        Assert.Equal("trace-123", body.TraceId);
        Assert.InRange(body.Timestamp, before, DateTimeOffset.UtcNow);
        ErrorResponseItem error = Assert.Single(body.Errors);
        Assert.Equal("VendorNotFound", error.Code);
        Assert.Equal("Vendor 7 was not found.", error.Message);
    }

    [Fact]
    public async Task InvokeAsync_DomainExceptionWithSeveralErrors_WritesEveryErrorInOrder()
    {
        var domainException = new DomainException(
            ErrorType.Validation,
            new DomainError { Code = "First", Message = "First problem." },
            new DomainError { Code = "Second", Message = "Second problem." });

        (DefaultHttpContext context, _) = await InvokeAsync(domainException);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal(["First", "Second"], ReadErrorResponse(context).Errors.Select(error => error.Code));
    }

    [Fact]
    public async Task InvokeAsync_DomainException_WritesTheCamelCaseWireShape()
    {
        var domainException = new DomainException(
            ErrorType.Validation,
            new DomainError { Code = "Required", Message = "Name is required." });

        (DefaultHttpContext context, _) = await InvokeAsync(domainException);

        using JsonDocument document = JsonDocument.Parse(ReadBody(context));
        JsonElement root = document.RootElement;
        Assert.Equal(["errors", "timestamp", "traceId"], PropertyNames(root));
        JsonElement error = Assert.Single(root.GetProperty("errors").EnumerateArray());
        Assert.Equal(["code", "message"], PropertyNames(error));

        static string[] PropertyNames(JsonElement element) =>
            [.. element.EnumerateObject().Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal)];
    }

    [Fact]
    public async Task InvokeAsync_DomainException_LogsWarningWithTheException()
    {
        var domainException = new DomainException(
            ErrorType.BusinessRule,
            new DomainError { Code = "VendorInactive", Message = "Vendor 7 is inactive." });

        (_, CapturingLogger logger) = await InvokeAsync(domainException);

        (LogLevel level, Exception? exception, _) = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, level);
        Assert.Same(domainException, exception);
    }

    [Fact]
    public async Task InvokeAsync_HttpRequestException_Writes502AndLogsWarning()
    {
        var httpRequestException = new HttpRequestException("Upstream said no.", null, HttpStatusCode.ServiceUnavailable);

        (DefaultHttpContext context, CapturingLogger logger) = await InvokeAsync(httpRequestException);

        Assert.Equal(StatusCodes.Status502BadGateway, context.Response.StatusCode);
        Assert.Equal("UpstreamServiceUnavailable", Assert.Single(ReadErrorResponse(context).Errors).Code);
        Assert.Equal(LogLevel.Warning, Assert.Single(logger.Entries).Level);
    }

    [Fact]
    public async Task InvokeAsync_HttpRequestExceptionWithNoStatusCode_StillWrites502()
    {
        (DefaultHttpContext context, _) = await InvokeAsync(new HttpRequestException("Could not connect."));

        Assert.Equal(StatusCodes.Status502BadGateway, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_HttpClientTimeout_Writes504AndLogsWarning()
    {
        var timeoutException = new TaskCanceledException("The request timed out.", new TimeoutException());

        (DefaultHttpContext context, CapturingLogger logger) = await InvokeAsync(timeoutException);

        Assert.Equal(StatusCodes.Status504GatewayTimeout, context.Response.StatusCode);
        Assert.Equal("UpstreamServiceTimeout", Assert.Single(ReadErrorResponse(context).Errors).Code);
        Assert.Equal(LogLevel.Warning, Assert.Single(logger.Entries).Level);
    }

    [Fact]
    public async Task InvokeAsync_CancellationNotCausedByTheCaller_FallsThroughToUnexpected500()
    {
        // No TimeoutException inside, and the request was not aborted, so neither special case applies.
        (DefaultHttpContext context, _) = await InvokeAsync(new TaskCanceledException("Cancelled."));

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("Unexpected", Assert.Single(ReadErrorResponse(context).Errors).Code);
    }

    [Fact]
    public async Task InvokeAsync_UnexpectedException_Writes500WithoutLeakingItsMessageAndLogsError()
    {
        var unexpected = new InvalidOperationException("Connection string is Server=secret;Password=hunter2.");

        (DefaultHttpContext context, CapturingLogger logger) = await InvokeAsync(unexpected);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        string body = ReadBody(context);
        Assert.DoesNotContain("hunter2", body);
        ErrorResponseItem error = Assert.Single(JsonSerializer.Deserialize<ErrorResponse>(body, WebOptions)!.Errors);
        Assert.Equal("Unexpected", error.Code);
        Assert.Equal("An unexpected error occurred.", error.Message);
        (LogLevel level, Exception? exception, string message) = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, level);
        Assert.Same(unexpected, exception);
        Assert.Contains(nameof(InvalidOperationException), message);
    }

    [Fact]
    public async Task InvokeAsync_CallerAbortedTheRequest_WritesNoBodyAndSetsClientClosedRequest()
    {
        using var aborted = new CancellationTokenSource();
        aborted.Cancel();

        (DefaultHttpContext context, CapturingLogger logger) = await InvokeAsync(
            new OperationCanceledException(aborted.Token),
            c => c.RequestAborted = aborted.Token);

        Assert.Equal(StatusCodes.Status499ClientClosedRequest, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
        Assert.Empty(logger.Entries);
    }

    [Fact]
    public async Task InvokeAsync_ResponseAlreadyStarted_RethrowsTheOriginalException()
    {
        var unexpected = new InvalidOperationException("Failed mid-stream.");
        var handler = new GlobalExceptionHandler(_ => throw unexpected, new CapturingLogger());
        DefaultHttpContext context = NewContext();
        context.Features.Set<IHttpResponseFeature>(new StartedResponseFeature());

        InvalidOperationException thrown = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.InvokeAsync(context));

        Assert.Same(unexpected, thrown);
        Assert.Equal(0, context.Response.Body.Length);
    }

    [Fact]
    public async Task InvokeAsync_RequestPathWithLineBreaks_LogsItOnOneLine()
    {
        (_, CapturingLogger logger) = await InvokeAsync(
            new InvalidOperationException("Boom."),
            c => c.Request.Path = "/vendors/1\r\nFAKE LOG LINE");

        (_, _, string message) = Assert.Single(logger.Entries);
        Assert.DoesNotContain('\n', message);
        Assert.DoesNotContain('\r', message);
        Assert.Contains("/vendors/1 FAKE LOG LINE", message);
    }

    private sealed class CapturingLogger : ILogger<GlobalExceptionHandler>
    {
        public List<(LogLevel Level, Exception? Exception, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, exception, formatter(state, exception)));
    }

    private sealed class StartedResponseFeature : HttpResponseFeature
    {
        public override bool HasStarted => true;
    }
}
