using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using PartsTrackerErp.GoldenPattern.Tests.Api.FailureBehavior;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.OpenApi;

/// <summary>
/// Keeps <c>src/api/openapi.json</c>, the file the front end generates its types from, in step with the OpenAPI
/// document the API serves. Run it with <c>UPDATE_OPENAPI_SNAPSHOT=true</c> to rewrite the file.
/// </summary>
[Trait(Constants.TestCategory, Constants.IntegrationTestCategory)]
[Collection(Constants.ApiPipelineCollection)]
public class Given_OpenApiSnapshot(ApiFactory factory)
{
    private const string UpdateVariable = "UPDATE_OPENAPI_SNAPSHOT";
    private const string SolutionPath = "src/api/PartsTrackerErp.GoldenPattern.slnx";
    private const string SnapshotPath = "src/api/openapi.json";

    private static readonly JsonSerializerOptions s_snapshotOptions = new() { WriteIndented = true, NewLine = "\n" };

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Document_MatchesTheSnapshotInSource()
    {
        string document = await GetSnapshotTextAsync();
        string path = Path.Combine(FindRepositoryRoot(), SnapshotPath);

        if (UpdateRequested())
        {
            await File.WriteAllTextAsync(path, document);
            return;
        }

        string committed = File.Exists(path) ? await File.ReadAllTextAsync(path) : string.Empty;

        Assert.True(
            committed == document,
            $"{SnapshotPath} is out of date. Run this test with {UpdateVariable}=true to rewrite it.");
    }

    private static bool UpdateRequested()
    {
        return bool.TryParse(Environment.GetEnvironmentVariable(UpdateVariable), out bool update) && update;
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, SolutionPath)))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException($"No folder above {AppContext.BaseDirectory} contains {SolutionPath}.");
    }

    private async Task<string> GetSnapshotTextAsync()
    {
        HttpResponseMessage response = await _client.GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // The descriptions come from the compiler's XML documentation, whose line breaks follow the platform.
        string text = (await response.Content.ReadAsStringAsync()).Replace("\\r\\n", "\\n");
        JsonObject document = JsonNode.Parse(text)!.AsObject();

        // The address of the test server says nothing about the contract.
        document.Remove("servers");

        return document.ToJsonString(s_snapshotOptions) + "\n";
    }
}
