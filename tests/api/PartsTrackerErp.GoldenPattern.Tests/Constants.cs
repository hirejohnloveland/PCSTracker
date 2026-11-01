namespace PartsTrackerErp.GoldenPattern.Tests;

internal static class Constants
{
    public const string TestCategory = "Category";
    public const string UnitTestCategory = "UnitTest";
    public const string IntegrationTestCategory = "IntegrationTest";

    // Cross-layer tests that boot the API against a real RavenDB server. 
    public const string EndToEndTestCategory = "EndToEnd";

    // Cross-layer tests that are designed to validate that the architecture is being followed. 
    // Use this category to test layer segregation, middleware or decorator use, inter module interaction, etc.
    public const string ArchitectureCategory = "Architecture";

    // Classes that send requests through the API in memory. They share one host (ApiPipelineCollection).
    public const string ApiPipelineCollection = "ApiPipeline";
}
