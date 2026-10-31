using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Exceptions;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace PartsTrackerErp.GoldenPattern.Core.Infrastructure.Persistence;

/// <summary>
/// Creates the configured RavenDB database when the host starts, if it does not already exist.
/// </summary>
internal sealed class RavenDbDatabaseInitializer(
    IDocumentStore store,
    ILogger<RavenDbDatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string database = store.Database;

        DatabaseRecordWithEtag? record = await store.Maintenance.Server.SendAsync(
            new GetDatabaseRecordOperation(database), cancellationToken);

        if (record is not null)
        {
            return;
        }

        try
        {
            await store.Maintenance.Server.SendAsync(
                new CreateDatabaseOperation(new DatabaseRecord(database)), cancellationToken);

            logger.LogInformation("Created RavenDB database '{Database}'.", database);
        }
        catch (ConcurrencyException)
        {
            // Another host created the database between the check and the create.
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
