namespace PartsTrackerErp.GoldenPattern.Tests.Api.FailureBehavior;

/// <summary>
/// Every class in the collection shares one in-memory API host. Two <see cref="ApiFactory"/> hosts in one test run
/// hung it; the cause is not known.
/// </summary>
[CollectionDefinition(Constants.ApiPipelineCollection)]
public sealed class ApiPipelineCollection : ICollectionFixture<ApiFactory>
{
}
