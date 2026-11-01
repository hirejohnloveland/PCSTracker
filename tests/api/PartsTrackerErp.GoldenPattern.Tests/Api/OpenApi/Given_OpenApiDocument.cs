using System.Net;
using System.Text;
using System.Text.Json;
using PartsTrackerErp.GoldenPattern.Tests.Api.FailureBehavior;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.OpenApi;

/// <summary>
/// Reads the OpenAPI document and the Swagger page the running API serves, in memory, and checks what they tell a
/// caller about the responses. Operations under <c>/testing</c> belong to the test fixtures and are skipped.
/// </summary>
[Trait(Constants.TestCategory, Constants.IntegrationTestCategory)]
[Collection(Constants.ApiPipelineCollection)]
public class Given_OpenApiDocument(ApiFactory factory)
{
    private const string JsonContentType = "application/json";
    private const string ProblemJson = "application/problem+json";
    private const string ProblemSchema = "ApiProblemDetails";
    private const string SchemaPrefix = "#/components/schemas/";

    private static readonly string[] s_httpMethods = ["get", "put", "post", "delete", "options", "head", "patch", "trace"];
    private static readonly string[] s_failureStatuses = ["400", "401", "403", "404", "409", "500"];

    // The operations that create a resource answer 201. Every other operation answers 200.
    private static readonly string[] s_creates = ["post /vendors", "post /vendorinvoices"];

    private readonly HttpClient _client = factory.CreateClient();

    private static List<(string Method, string Path, JsonElement Operation)> OperationsOf(JsonElement document)
    {
        List<(string Method, string Path, JsonElement Operation)> operations = [];
        foreach (JsonProperty path in document.GetProperty("paths").EnumerateObject())
        {
            if (path.Name.StartsWith("/testing", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (JsonProperty operation in path.Value.EnumerateObject().Where(member => s_httpMethods.Contains(member.Name)))
            {
                operations.Add((operation.Name, path.Name, operation.Value));
            }
        }

        return operations;
    }

    private static string? SchemaReferenceOf(JsonElement operation, string status, string contentType = JsonContentType)
    {
        return operation.GetProperty("responses").TryGetProperty(status, out JsonElement response)
            && response.TryGetProperty("content", out JsonElement content)
            && content.TryGetProperty(contentType, out JsonElement media)
            && media.TryGetProperty("schema", out JsonElement schema)
            && schema.TryGetProperty("$ref", out JsonElement reference)
                ? reference.GetString()
                : null;
    }

    private static JsonElement OperationNamed(JsonElement document, string method, string path)
    {
        Dictionary<string, JsonElement> operations = OperationsOf(document)
            .ToDictionary(item => $"{item.Method} {item.Path}", item => item.Operation, StringComparer.OrdinalIgnoreCase);

        Assert.True(operations.TryGetValue($"{method} {path}", out JsonElement operation), $"The document has no {method} {path}.");
        return operation;
    }

    private static JsonElement SchemaReferredBy(JsonElement document, string? reference)
    {
        Assert.NotNull(reference);
        Assert.StartsWith(SchemaPrefix, reference);

        return document.GetProperty("components").GetProperty("schemas").GetProperty(reference[SchemaPrefix.Length..]);
    }

    private static string[] PropertyNamesOf(JsonElement schema)
    {
        return [.. schema.GetProperty("properties").EnumerateObject().Select(property => property.Name)];
    }

    private static string[] MemberNamesOf(JsonElement element)
    {
        return [.. element.EnumerateObject().Select(member => member.Name)];
    }

    private async Task<JsonElement> GetDocumentAsync()
    {
        HttpResponseMessage response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.Clone();
    }

    [Fact]
    public async Task EveryOperation_DeclaresItsSuccessWithAJsonSchema()
    {
        List<(string Method, string Path, JsonElement Operation)> operations = OperationsOf(await GetDocumentAsync());
        List<string> problems = [];

        foreach ((string method, string path, JsonElement operation) in operations)
        {
            string name = $"{method} {path}";
            string expected = s_creates.Contains(name, StringComparer.OrdinalIgnoreCase) ? "201" : "200";
            JsonElement responses = operation.GetProperty("responses");

            string[] successes = [.. responses.EnumerateObject().Select(response => response.Name).Where(status => status.StartsWith('2'))];
            if (!successes.SequenceEqual([expected]))
            {
                problems.Add($"{name} declares [{string.Join(", ", successes)}] and not [{expected}]");
            }
            else if (SchemaReferenceOf(operation, expected) is null)
            {
                problems.Add($"{name} has no application/json schema for its {expected}");
            }
        }

        Assert.NotEmpty(operations);
        Assert.Empty(problems);
    }

    [Fact]
    public async Task EveryOperation_DeclaresTheSixFailuresAsProblemJson()
    {
        List<(string Method, string Path, JsonElement Operation)> operations = OperationsOf(await GetDocumentAsync());
        List<string> problems = [];

        foreach ((string method, string path, JsonElement operation) in operations)
        {
            JsonElement responses = operation.GetProperty("responses");
            foreach (string status in s_failureStatuses)
            {
                if (!responses.TryGetProperty(status, out JsonElement response) || !response.TryGetProperty("content", out JsonElement content))
                {
                    problems.Add($"{method} {path} has no {status} response with content");
                    continue;
                }

                string[] contentTypes = [.. content.EnumerateObject().Select(type => type.Name)];
                if (!contentTypes.SequenceEqual([ProblemJson]))
                {
                    problems.Add($"{method} {path} offers [{string.Join(", ", contentTypes)}] for its {status}");
                }

                if (SchemaReferenceOf(operation, status, ProblemJson) != $"{SchemaPrefix}{ProblemSchema}")
                {
                    problems.Add($"{method} {path} does not describe its {status} as {ProblemSchema}");
                }
            }
        }

        Assert.NotEmpty(operations);
        Assert.Empty(problems);
    }

    [Theory]
    [InlineData("get", "/vendors", "200", "PagedApiResponseOfVendorResponseDto")]
    [InlineData("get", "/vendors/{id}", "200", "ApiResponseOfVendorResponseDto")]
    [InlineData("post", "/vendors", "201", "ApiResponseOfVendorResponseDto")]
    public async Task Operation_SuccessRefersToItsEnvelope(string method, string path, string status, string schema)
    {
        JsonElement operation = OperationNamed(await GetDocumentAsync(), method, path);

        Assert.Equal($"{SchemaPrefix}{schema}", SchemaReferenceOf(operation, status));
    }

    [Fact]
    public async Task ProblemSchema_ListsTheTraceIdAndTheErrors()
    {
        JsonElement document = await GetDocumentAsync();
        JsonElement problem = SchemaReferredBy(document, $"{SchemaPrefix}{ProblemSchema}");
        string[] required = [.. problem.GetProperty("required").EnumerateArray().Select(name => name.GetString()!)];

        Assert.Contains("traceId", PropertyNamesOf(problem));
        Assert.Contains("errors", PropertyNamesOf(problem));
        Assert.Contains("traceId", required);
        Assert.Contains("errors", required);
        Assert.Equal(
            ["code", "detail", "pointer"],
            PropertyNamesOf(SchemaReferredBy(document, $"{SchemaPrefix}ProblemError")).Order());
    }

    public static TheoryData<string, string, string, string, string?> RealFailures()
    {
        return new()
        {
            { "GET", "/vendors/{id}", "404", "/vendors/vendors-1-A", null },
            { "POST", "/vendors", "400", "/vendors", """{"name":""}""" }
        };
    }

    [Theory]
    [MemberData(nameof(RealFailures))]
    public async Task RealFailure_EveryMember_IsDescribedByTheDocument(
        string method,
        string path,
        string status,
        string url,
        string? body)
    {
        JsonElement document = await GetDocumentAsync();
        JsonElement problemSchema = SchemaReferredBy(
            document,
            SchemaReferenceOf(OperationNamed(document, method, path), status, ProblemJson));

        using var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (body is not null)
        {
            request.Content = new StringContent(body, Encoding.UTF8, JsonContentType);
        }

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(int.Parse(status), (int)response.StatusCode);
        using JsonDocument problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Empty(MemberNamesOf(problem.RootElement).Except(PropertyNamesOf(problemSchema)));

        JsonElement errorSchema = SchemaReferredBy(
            document,
            problemSchema.GetProperty("properties").GetProperty("errors").GetProperty("items").GetProperty("$ref").GetString());
        Assert.Empty(problem.RootElement.GetProperty("errors").EnumerateArray()
            .SelectMany(MemberNamesOf)
            .Except(PropertyNamesOf(errorSchema)));
    }

    [Fact]
    public async Task SwaggerUi_ServesItsPage()
    {
        HttpResponseMessage response = await _client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
