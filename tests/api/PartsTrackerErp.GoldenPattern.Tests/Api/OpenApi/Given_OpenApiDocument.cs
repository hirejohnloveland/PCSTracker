using System.Net;
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
    private const string ProblemJson = "application/problem+json";

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

    private static string? SchemaReferenceOf(JsonElement operation, string status)
    {
        return operation.GetProperty("responses").TryGetProperty(status, out JsonElement response)
            && response.TryGetProperty("content", out JsonElement content)
            && content.TryGetProperty("application/json", out JsonElement json)
            && json.TryGetProperty("schema", out JsonElement schema)
            && schema.TryGetProperty("$ref", out JsonElement reference)
                ? reference.GetString()
                : null;
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
        Dictionary<string, JsonElement> operations = OperationsOf(await GetDocumentAsync())
            .ToDictionary(item => $"{item.Method} {item.Path}", item => item.Operation, StringComparer.OrdinalIgnoreCase);

        Assert.True(operations.TryGetValue($"{method} {path}", out JsonElement operation), $"The document has no {method} {path}.");
        Assert.Equal($"#/components/schemas/{schema}", SchemaReferenceOf(operation, status));
    }

    [Fact]
    public async Task SwaggerUi_ServesItsPage()
    {
        HttpResponseMessage response = await _client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
