using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.FailureBehavior;

/// <summary>
/// Sends real requests through the whole pipeline, in memory, and checks what a caller receives when a request fails.
/// </summary>
[Trait(Constants.TestCategory, Constants.IntegrationTestCategory)]
public class Given_ApiFailureBehavior : IClassFixture<ApiFactory>
{
    private const string FailuresPath = "/testing/failures";
    private const string TypeBase = "https://api.example.com/problems/";

    private static readonly Dictionary<ErrorType, (HttpStatusCode Status, string Category)> s_categories = new()
    {
        [ErrorType.Validation] = (HttpStatusCode.BadRequest, "validation"),
        [ErrorType.Unauthorized] = (HttpStatusCode.Unauthorized, "unauthorized"),
        [ErrorType.Forbidden] = (HttpStatusCode.Forbidden, "forbidden"),
        [ErrorType.NotFound] = (HttpStatusCode.NotFound, "not-found"),
        [ErrorType.BusinessRuleViolation] = (HttpStatusCode.Conflict, "business-rule-violation"),
        [ErrorType.Unexpected] = (HttpStatusCode.InternalServerError, "unexpected")
    };

    private readonly ApiFactory _factory;
    private readonly FailureTestingService _service;
    private readonly HttpClient _client;

    public Given_ApiFailureBehavior(ApiFactory factory)
    {
        _factory = factory;
        _service = factory.TestingService;
        _service.Reset();
        _client = factory.CreateClient();
    }

    public static TheoryData<ErrorType, HttpStatusCode, string> CategoryCases()
    {
        var cases = new TheoryData<ErrorType, HttpStatusCode, string>();
        foreach ((ErrorType errorType, (HttpStatusCode status, string category)) in s_categories)
        {
            cases.Add(errorType, status, category);
        }

        return cases;
    }

    private static DomainError Error(string code, string message)
    {
        return new() { Code = code, Message = message };
    }

    private static StringContent Json(string json)
    {
        return new(json, Encoding.UTF8, "application/json");
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.Clone();
    }

    private static JsonElement[] ErrorsOf(JsonElement problem)
    {
        return [.. problem.GetProperty("errors").EnumerateArray()];
    }

    private static string[] PropertyNames(JsonElement element)
    {
        return [.. element.EnumerateObject().Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal)];
    }

    /// <summary>What every failure has in common, whatever caused it.</summary>
    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response, HttpStatusCode expectedStatus)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        JsonElement problem = await ReadJsonAsync(response);
        Assert.Equal((int)expectedStatus, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrEmpty(problem.GetProperty("traceId").GetString()));
        Assert.False(string.IsNullOrEmpty(problem.GetProperty("instance").GetString()));
        Assert.Equal(JsonValueKind.Array, problem.GetProperty("errors").ValueKind);
        Assert.All(
            ErrorsOf(problem),
            error =>
            {
                Assert.False(string.IsNullOrEmpty(error.GetProperty("code").GetString()));
                Assert.False(string.IsNullOrEmpty(error.GetProperty("detail").GetString()));
            });

        return problem;
    }

    private static async Task<JsonNode> ReadNodeWithoutTraceIdAsync(HttpResponseMessage response)
    {
        JsonNode node = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        node.AsObject().Remove("traceId");
        return node;
    }

    // ---- Success ----

    [Fact]
    public async Task Success_WritesTheDataAndMetadata()
    {
        HttpResponseMessage response = await _client.GetAsync(FailuresPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        JsonElement body = await ReadJsonAsync(response);
        Assert.Equal(["data", "metadata"], PropertyNames(body));
        Assert.Equal("ok", body.GetProperty("data").GetString());
        JsonElement metadata = body.GetProperty("metadata");
        Assert.Equal(["timestamp", "traceId"], PropertyNames(metadata));
        Assert.False(string.IsNullOrEmpty(metadata.GetProperty("traceId").GetString()));
        Assert.EndsWith("Z", metadata.GetProperty("timestamp").GetString());
    }

    // ---- A service reports a failure ----

    [Theory]
    [MemberData(nameof(CategoryCases))]
    public async Task ServiceFailure_ReturnsTheProblemForItsCategory(ErrorType errorType, HttpStatusCode status, string category)
    {
        _service.Respond(ServiceResponse<string>.Fail(errorType, Error("SomeCode", "Something went wrong.")));

        JsonElement problem = await ReadProblemAsync(await _client.GetAsync(FailuresPath), status);

        Assert.Equal(TypeBase + category, problem.GetProperty("type").GetString());
        Assert.Equal(FailuresPath, problem.GetProperty("instance").GetString());
        Assert.Equal("Something went wrong.", problem.GetProperty("detail").GetString());
        JsonElement error = Assert.Single(ErrorsOf(problem));
        Assert.Equal("SomeCode", error.GetProperty("code").GetString());
        Assert.Equal("Something went wrong.", error.GetProperty("detail").GetString());
        Assert.False(error.TryGetProperty("pointer", out _));
    }

    [Fact]
    public async Task ServiceFailure_SeveralErrors_ListsEachInOrderUnderAGenericDetail()
    {
        _service.Respond(ServiceResponse<string>.Fail(
            ErrorType.BusinessRuleViolation,
            Error("First", "First problem."),
            Error("Second", "Second problem.")));

        JsonElement problem = await ReadProblemAsync(await _client.GetAsync(FailuresPath), HttpStatusCode.Conflict);

        Assert.Equal("Conflict", problem.GetProperty("title").GetString());
        Assert.Equal("One or more errors occurred.", problem.GetProperty("detail").GetString());
        Assert.Equal(
            ["First", "Second"],
            ErrorsOf(problem).Select(error => error.GetProperty("code").GetString()));
    }

    [Fact]
    public async Task ServiceFailure_UndefinedErrorType_Returns500()
    {
        _service.Respond(ServiceResponse<string>.Fail((ErrorType)999, Error("SomeCode", "Something went wrong.")));

        JsonElement problem = await ReadProblemAsync(await _client.GetAsync(FailuresPath), HttpStatusCode.InternalServerError);

        Assert.Equal(TypeBase + "unexpected", problem.GetProperty("type").GetString());
    }

    // ---- A service throws ----

    [Theory]
    [MemberData(nameof(CategoryCases))]
    public async Task ThrownDomainException_ReturnsTheSameProblemAsAServiceFailure(
        ErrorType errorType,
        HttpStatusCode status,
        string category)
    {
        DomainError[] errors = [Error("First", "First problem."), Error("Second", "Second problem.")];

        _service.Respond(ServiceResponse<string>.Fail(errorType, errors));
        HttpResponseMessage failed = await _client.GetAsync(FailuresPath);
        _service.Throw(new DomainException(errorType, errors));
        HttpResponseMessage thrown = await _client.GetAsync(FailuresPath);

        JsonElement problem = await ReadProblemAsync(thrown, status);
        Assert.Equal(TypeBase + category, problem.GetProperty("type").GetString());
        JsonNode failedBody = await ReadNodeWithoutTraceIdAsync(failed);
        JsonNode thrownBody = await ReadNodeWithoutTraceIdAsync(thrown);
        Assert.True(
            JsonNode.DeepEquals(failedBody, thrownBody),
            $"A service failure and a thrown exception should give the same problem.\nFailure: {failedBody}\nException: {thrownBody}");
    }

    [Fact]
    public async Task ThrownUnexpectedException_Returns500WithoutLeakingItsMessage()
    {
        _service.Throw(new InvalidOperationException("Connection string is Server=secret;Password=hunter2."));

        HttpResponseMessage response = await _client.GetAsync(FailuresPath);

        JsonElement problem = await ReadProblemAsync(response, HttpStatusCode.InternalServerError);
        Assert.DoesNotContain("hunter2", problem.GetRawText());
        Assert.Equal(TypeBase + "unexpected", problem.GetProperty("type").GetString());
        JsonElement error = Assert.Single(ErrorsOf(problem));
        Assert.Equal("Unexpected", error.GetProperty("code").GetString());
        Assert.Equal("An unexpected error occurred.", error.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task ThrownHttpRequestException_Returns502WithAFixedMessage()
    {
        _service.Throw(new HttpRequestException("Upstream said no.", null, HttpStatusCode.ServiceUnavailable));

        JsonElement problem = await ReadProblemAsync(await _client.GetAsync(FailuresPath), HttpStatusCode.BadGateway);

        Assert.DoesNotContain("Upstream said no.", problem.GetRawText());
        Assert.Equal("UpstreamServiceUnavailable", Assert.Single(ErrorsOf(problem)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task ThrownHttpClientTimeout_Returns504()
    {
        _service.Throw(new TaskCanceledException("The request timed out.", new TimeoutException()));

        JsonElement problem = await ReadProblemAsync(await _client.GetAsync(FailuresPath), HttpStatusCode.GatewayTimeout);

        Assert.Equal("UpstreamServiceTimeout", Assert.Single(ErrorsOf(problem)).GetProperty("code").GetString());
    }

    [Theory]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.RequestEntityTooLarge)]
    public async Task ThrownBadHttpRequest_KeepsItsOwnStatusAndLeaksNoExceptionText(HttpStatusCode status)
    {
        _service.Throw(new BadHttpRequestException("Server=secret;Password=hunter2.", (int)status));

        JsonElement problem = await ReadProblemAsync(await _client.GetAsync(FailuresPath), status);

        Assert.DoesNotContain("hunter2", problem.GetRawText());
        Assert.Equal("InvalidRequest", Assert.Single(ErrorsOf(problem)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task ThrownCancellation_WhenTheCallerAborted_Answers499WithNoBody()
    {
        using var aborted = new CancellationTokenSource();
        aborted.Cancel();
        _service.Throw(new OperationCanceledException(aborted.Token));

        // HttpClient cannot send a request that is already aborted, so drive the server directly.
        HttpContext context = await _factory.Server.SendAsync(httpContext =>
        {
            httpContext.Request.Method = HttpMethods.Get;
            httpContext.Request.Path = FailuresPath;
            httpContext.RequestAborted = aborted.Token;
        });

        Assert.Equal(StatusCodes.Status499ClientClosedRequest, context.Response.StatusCode);
        Assert.Null(context.Response.ContentType);
    }

    [Fact]
    public async Task ThrownDomainException_WhenTheProblemWriterDeclines_StillAnswersWithItsStatus()
    {
        _service.Throw(new DomainException(ErrorType.NotFound, Error("VendorNotFound", "Vendor 7 was not found.")));
        using var request = new HttpRequestMessage(HttpMethod.Get, "/testing/plain");
        request.Headers.Accept.ParseAdd("text/html");

        HttpResponseMessage response = await _client.SendAsync(request);

        // The default writer declines an Accept header that excludes JSON. A handler that then returned false
        // would trip the exception middleware's 404 guard, and the caller would get a 500.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task ThrownDomainException_FromAnApiControllerWhoseCallerDoesNotAcceptJson_StillAnswersWithTheProblem()
    {
        _service.Throw(new DomainException(ErrorType.NotFound, Error("VendorNotFound", "Vendor 7 was not found.")));
        using var request = new HttpRequestMessage(HttpMethod.Get, FailuresPath);
        request.Headers.Accept.ParseAdd("text/html");

        HttpResponseMessage response = await _client.SendAsync(request);

        JsonElement problem = await ReadProblemAsync(response, HttpStatusCode.NotFound);
        Assert.Equal("VendorNotFound", Assert.Single(ErrorsOf(problem)).GetProperty("code").GetString());
    }

    // ---- The request is invalid ----

    [Fact]
    public async Task InvalidRequest_ListsEveryValidationFailureWithAPointerAndSkipsTheService()
    {
        HttpResponseMessage response = await _client.PostAsync(
            FailuresPath,
            Json("""{"name":"","email":"not-an-email","items":[{"sku":"A-1"},{"sku":""}]}"""));

        JsonElement problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.Equal(TypeBase + "validation", problem.GetProperty("type").GetString());
        Assert.Equal("Validation Failed", problem.GetProperty("title").GetString());
        Assert.Equal("One or more validation errors occurred.", problem.GetProperty("detail").GetString());
        Assert.Equal(
            [
                ("NotEmptyValidator", "Name is required.", "#/name"),
                ("EmailValidator", "Email address is invalid.", "#/email"),
                ("NotEmptyValidator", "Sku is required.", "#/items/1/sku")
            ],
            ErrorsOf(problem).Select(error => (
                error.GetProperty("code").GetString(),
                error.GetProperty("detail").GetString(),
                error.GetProperty("pointer").GetString())));
        Assert.Equal(0, _service.Calls);
    }

    [Fact]
    public async Task ValidRequest_CallsTheServiceAndAnswersWithItsData()
    {
        HttpResponseMessage response = await _client.PostAsync(FailuresPath, Json("""{"name":"Acme","email":"a@b.example"}"""));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ok", (await ReadJsonAsync(response)).GetProperty("data").GetString());
        Assert.Equal(1, _service.Calls);
    }

    [Fact]
    public async Task MalformedJson_ReturnsInvalidRequestEntriesInsteadOfTheFrameworksFieldKeyedErrors()
    {
        HttpResponseMessage response = await _client.PostAsync(FailuresPath, Json("{ nope"));

        JsonElement problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.Equal(TypeBase + "validation", problem.GetProperty("type").GetString());
        Assert.All(
            ErrorsOf(problem),
            error =>
            {
                Assert.Equal("InvalidRequest", error.GetProperty("code").GetString());
                Assert.True(error.TryGetProperty("pointer", out _));
            });
        Assert.Equal(0, _service.Calls);
    }

    [Fact]
    public async Task WrongJsonType_PointsAtTheProperty()
    {
        HttpResponseMessage response = await _client.PostAsync(FailuresPath, Json("""{"name":"Acme","count":"abc"}"""));

        JsonElement problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.Contains(
            ErrorsOf(problem),
            error => error.GetProperty("pointer").GetString() == "#/count"
                && error.GetProperty("code").GetString() == "InvalidRequest");
    }

    // ---- The framework fails the request ----

    [Fact]
    public async Task UnknownRoute_Returns404WithTheDefaultNotFoundEntry()
    {
        JsonElement problem = await ReadProblemAsync(await _client.GetAsync("/no-such-route"), HttpStatusCode.NotFound);

        Assert.Equal(TypeBase + "not-found", problem.GetProperty("type").GetString());
        Assert.Equal("/no-such-route", problem.GetProperty("instance").GetString());
        Assert.Equal("NotFound", Assert.Single(ErrorsOf(problem)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task WrongMethod_Returns405KeepingTheFrameworkType()
    {
        JsonElement problem = await ReadProblemAsync(await _client.DeleteAsync(FailuresPath), HttpStatusCode.MethodNotAllowed);

        Assert.DoesNotContain(TypeBase, problem.GetProperty("type").GetString());
        Assert.Equal("InvalidRequest", Assert.Single(ErrorsOf(problem)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task UnsupportedMediaType_Returns415KeepingTheFrameworkType()
    {
        using var content = new StringContent("hello", Encoding.UTF8, "text/plain");

        JsonElement problem = await ReadProblemAsync(
            await _client.PostAsync(FailuresPath, content),
            HttpStatusCode.UnsupportedMediaType);

        Assert.DoesNotContain(TypeBase, problem.GetProperty("type").GetString());
        Assert.Equal("InvalidRequest", Assert.Single(ErrorsOf(problem)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Unauthenticated_Returns401WithTheDefaultUnauthorizedEntry()
    {
        JsonElement problem = await ReadProblemAsync(
            await _client.GetAsync($"{FailuresPath}/secured"),
            HttpStatusCode.Unauthorized);

        Assert.Equal(TypeBase + "unauthorized", problem.GetProperty("type").GetString());
        Assert.Equal("Unauthorized", Assert.Single(ErrorsOf(problem)).GetProperty("code").GetString());
    }

    // ---- The real vendor endpoints ----

    [Fact]
    public async Task Vendors_ActivateMissingVendor_Returns404WithVendorNotFound()
    {
        JsonElement problem = await ReadProblemAsync(
            await _client.PostAsync("/vendors/abc/activate", content: null),
            HttpStatusCode.NotFound);

        Assert.Equal(TypeBase + "not-found", problem.GetProperty("type").GetString());
        Assert.Equal("/vendors/abc/activate", problem.GetProperty("instance").GetString());
        JsonElement error = Assert.Single(ErrorsOf(problem));
        Assert.Equal("VendorNotFound", error.GetProperty("code").GetString());
        Assert.Equal("Vendor 'abc' was not found.", error.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Vendors_AddWithNoName_Returns400PointingAtTheName()
    {
        JsonElement problem = await ReadProblemAsync(
            await _client.PostAsync("/vendors", Json("""{"name":""}""")),
            HttpStatusCode.BadRequest);

        JsonElement error = Assert.Single(ErrorsOf(problem));
        Assert.Equal("NotEmptyValidator", error.GetProperty("code").GetString());
        Assert.Equal("Vendor name is required.", error.GetProperty("detail").GetString());
        Assert.Equal("#/name", error.GetProperty("pointer").GetString());
    }

    [Fact]
    public async Task Vendors_AddWithAName_AnswersWithTheVendorAndMetadata()
    {
        HttpResponseMessage response = await _client.PostAsync("/vendors", Json("""{"name":"Acme"}"""));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement body = await ReadJsonAsync(response);
        Assert.Equal("Acme", body.GetProperty("data").GetProperty("name").GetString());
        Assert.True(body.GetProperty("data").GetProperty("isActive").GetBoolean());
        Assert.False(string.IsNullOrEmpty(body.GetProperty("metadata").GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task VendorInvoices_ValidInvoice_AnswersWithTheInvoiceAndMetadata()
    {
        HttpResponseMessage response = await _client.PostAsync("/vendorinvoices", Json("""{"vendorName":"Acme","balance":10.5}"""));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement body = await ReadJsonAsync(response);
        Assert.Equal("Acme", body.GetProperty("data").GetProperty("vendorName").GetString());
        Assert.Equal(10.5m, body.GetProperty("data").GetProperty("balance").GetDecimal());
        Assert.False(string.IsNullOrEmpty(body.GetProperty("metadata").GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task VendorInvoices_InvalidInvoice_ListsBothFailuresWithPointers()
    {
        JsonElement problem = await ReadProblemAsync(
            await _client.PostAsync("/vendorinvoices", Json("""{"vendorName":"","balance":0}""")),
            HttpStatusCode.BadRequest);

        Assert.Equal(
            [("NotEmptyValidator", "#/vendorName"), ("GreaterThanValidator", "#/balance")],
            ErrorsOf(problem).Select(error => (
                error.GetProperty("code").GetString(),
                error.GetProperty("pointer").GetString())));
    }
}
