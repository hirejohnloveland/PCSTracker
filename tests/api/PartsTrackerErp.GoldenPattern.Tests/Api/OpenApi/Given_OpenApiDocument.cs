using System.Net;
using System.Text;
using System.Text.Json;
using PartsTrackerErp.GoldenPattern.Tests.Api.FailureBehavior;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.OpenApi;

/// <summary>
/// Reads the OpenAPI document and the Swagger page the running API serves, in memory, and checks what they tell a
/// caller about the requests and the responses.
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
    private static readonly string[] s_creates = ["post /vendors", "post /customers", "post /vendorinvoices"];

    private readonly HttpClient _client = factory.CreateClient();

    private static List<(string Method, string Path, JsonElement Operation)> OperationsOf(JsonElement document)
    {
        List<(string Method, string Path, JsonElement Operation)> operations = [];
        foreach (JsonProperty path in document.GetProperty("paths").EnumerateObject())
        {
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

    private static string[] RequiredNamesOf(JsonElement schema)
    {
        return schema.TryGetProperty("required", out JsonElement required)
            ? [.. required.EnumerateArray().Select(name => name.GetString()!)]
            : [];
    }

    // The generator writes a nullable schema as a type that includes "null", or as a oneOf with a null alternative.
    private static bool OffersNull(JsonElement schema)
    {
        if (schema.TryGetProperty("type", out JsonElement type))
        {
            string?[] types = type.ValueKind == JsonValueKind.Array
                ? [.. type.EnumerateArray().Select(item => item.GetString())]
                : [type.GetString()];
            if (types.Contains("null"))
            {
                return true;
            }
        }

        return schema.TryGetProperty("oneOf", out JsonElement alternatives) && alternatives.EnumerateArray().Any(OffersNull);
    }

    private static void CollectNumbersOfferedAsStrings(JsonElement element, string location, List<string> offers)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty member in element.EnumerateObject())
                {
                    string memberLocation = $"{location}/{member.Name}";
                    if (member.Name == "type" && member.Value.ValueKind == JsonValueKind.Array)
                    {
                        string?[] types = [.. member.Value.EnumerateArray().Select(type => type.GetString())];
                        if (types.Contains("string") && (types.Contains("integer") || types.Contains("number")))
                        {
                            offers.Add(memberLocation);
                        }
                    }
                    else
                    {
                        CollectNumbersOfferedAsStrings(member.Value, memberLocation, offers);
                    }
                }

                break;
            case JsonValueKind.Array:
                for (int index = 0; index < element.GetArrayLength(); index++)
                {
                    CollectNumbersOfferedAsStrings(element[index], $"{location}[{index}]", offers);
                }

                break;
        }
    }

    private static IEnumerable<string> QueryParameterNamesOf(JsonElement operation)
    {
        if (!operation.TryGetProperty("parameters", out JsonElement parameters))
        {
            return [];
        }

        return parameters.EnumerateArray()
            .Where(parameter => parameter.GetProperty("in").GetString() == "query")
            .Select(parameter => parameter.GetProperty("name").GetString()!);
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
    [InlineData("get", "/customers", "200", "PagedApiResponseOfCustomerResponseDto")]
    [InlineData("get", "/customers/{id}", "200", "ApiResponseOfCustomerResponseDto")]
    [InlineData("post", "/customers", "201", "ApiResponseOfCustomerResponseDto")]
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

    public static TheoryData<string, string[]> RequestSchemas()
    {
        return new()
        {
            { "VendorDto", ["name"] },
            { "CustomerDto", ["name"] },
            { "VendorInvoiceDto", ["vendorName", "balance"] }
        };
    }

    [Theory]
    [MemberData(nameof(RequestSchemas))]
    public async Task RequestSchema_ListsItsMandatoryPropertiesAsRequired(string schema, string[] required)
    {
        JsonElement document = await GetDocumentAsync();

        Assert.Equal(required.Order(), RequiredNamesOf(SchemaReferredBy(document, $"{SchemaPrefix}{schema}")).Order());
    }

    [Fact]
    public async Task VendorResponseSchema_NeverOffersANullId()
    {
        JsonElement document = await GetDocumentAsync();
        JsonElement vendor = SchemaReferredBy(document, $"{SchemaPrefix}VendorResponseDto");

        Assert.False(OffersNull(vendor.GetProperty("properties").GetProperty("id")));
    }

    [Fact]
    public async Task CustomerResponseSchema_NeverOffersANullId()
    {
        JsonElement document = await GetDocumentAsync();
        JsonElement customer = SchemaReferredBy(document, $"{SchemaPrefix}CustomerResponseDto");

        Assert.False(OffersNull(customer.GetProperty("properties").GetProperty("id")));
    }

    [Fact]
    public async Task SuccessEnvelopes_NeverOfferNullData()
    {
        JsonElement document = await GetDocumentAsync();
        JsonProperty[] envelopes = [.. document.GetProperty("components").GetProperty("schemas").EnumerateObject()
            .Where(schema => schema.Name.Contains("ApiResponseOf", StringComparison.Ordinal))];

        string[] offers = [.. envelopes
            .Where(envelope => OffersNull(envelope.Value.GetProperty("properties").GetProperty("data")))
            .Select(envelope => envelope.Name)];

        Assert.NotEmpty(envelopes);
        Assert.Empty(offers);
    }

    [Fact]
    public async Task Document_NeverOffersANumberAsAString()
    {
        List<string> offers = [];

        CollectNumbersOfferedAsStrings(await GetDocumentAsync(), "#", offers);

        Assert.Empty(offers);
    }

    [Fact]
    public async Task EveryQueryParameter_IsNamedInCamelCase()
    {
        List<string> names = [.. OperationsOf(await GetDocumentAsync()).SelectMany(item => QueryParameterNamesOf(item.Operation))];

        Assert.NotEmpty(names);
        Assert.DoesNotContain(names, name => name != JsonNamingPolicy.CamelCase.ConvertName(name));
    }

    public static TheoryData<string, string, string, string, string?> RealFailures()
    {
        return new()
        {
            { "GET", "/vendors/{id}", "404", "/vendors/vendors-1-A", null },
            { "POST", "/vendors", "400", "/vendors", """{"name":""}""" },
            { "POST", "/vendors", "400", "/vendors", "{}" },
            { "GET", "/customers/{id}", "404", "/customers/customers-1-A", null },
            { "POST", "/customers", "400", "/customers", """{"name":""}""" },
            { "POST", "/customers", "400", "/customers", "{}" },
            { "POST", "/vendorinvoices", "400", "/vendorinvoices", "{}" }
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
