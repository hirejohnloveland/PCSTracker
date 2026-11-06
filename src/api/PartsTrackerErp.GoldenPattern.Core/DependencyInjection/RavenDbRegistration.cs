using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities.Settings;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;

namespace PartsTrackerErp.GoldenPattern.Core.DependencyInjection;

internal static class RavenDbRegistration
{
    internal static IHostApplicationBuilder AddCoreRavenDb(this IHostApplicationBuilder builder)
    {
        // One document store per process.
        builder.Services.AddSingleton<IDocumentStore>(sp =>
        {
            IConfigurationSection section = sp.GetRequiredService<IConfiguration>().GetSection("RavenDb");

            return new DocumentStore
            {
                Urls = [.. section.GetSection("Urls").GetChildren().Select(url => url.Value!)],
                Database = section["Database"],
                // Ids look like vendors-1-A. The default separator puts a slash in the id, which a route parameter cannot hold.
                Conventions =
                {
                    IdentityPartsSeparator = '-',
                    // Every settings document lives in one Settings collection, whatever its class.
                    FindCollectionName = type => typeof(ISettingsDocument).IsAssignableFrom(type)
                        ? "Settings"
                        : DocumentConventions.DefaultGetCollectionName(type)
                }
            }.Initialize();
        });

        // One session per scope, shared by every repository in that scope.
        builder.Services.AddScoped(sp => sp.GetRequiredService<IDocumentStore>().OpenAsyncSession());

        return builder;
    }
}
