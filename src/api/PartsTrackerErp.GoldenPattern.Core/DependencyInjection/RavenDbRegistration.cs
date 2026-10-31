using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PartsTrackerErp.GoldenPattern.Core.Infrastructure.Persistence;
using Raven.Client.Documents;

namespace PartsTrackerErp.GoldenPattern.Core.DependencyInjection;

internal static class RavenDbRegistration
{
    internal static IHostApplicationBuilder AddCoreRavenDb(this IHostApplicationBuilder builder)
    {
        // One document store per process.
        builder.Services.AddSingleton<IDocumentStore>(sp =>
        {
            var section = sp.GetRequiredService<IConfiguration>().GetSection("RavenDb");

            return new DocumentStore
            {
                Urls = [.. section.GetSection("Urls").GetChildren().Select(url => url.Value!)],
                Database = section["Database"]
            }.Initialize();
        });

        // One session per scope, shared by every repository in that scope.
        builder.Services.AddScoped(sp => sp.GetRequiredService<IDocumentStore>().OpenAsyncSession());

        // Creates the database at startup if it does not exist.
        builder.Services.AddHostedService<RavenDbDatabaseInitializer>();

        return builder;
    }
}
