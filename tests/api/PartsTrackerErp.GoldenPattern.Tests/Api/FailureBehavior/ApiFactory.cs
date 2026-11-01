using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.FailureBehavior;

/// <summary>
/// Boots the real API pipeline in memory, without a RavenDB server. Adds <see cref="FailureTestingController"/>.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public FailureTestingService TestingService => Services.GetRequiredService<FailureTestingService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // The initializer connects to RavenDB at startup.
            foreach (ServiceDescriptor initializer in services
                .Where(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType?.Name == "RavenDbDatabaseInitializer")
                .ToList())
            {
                services.Remove(initializer);
            }

            services.RemoveAll<IVendorRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IVendorRepository, EmptyVendorRepository>();
            services.AddScoped<IUnitOfWork, NoOpUnitOfWork>();

            services.AddSingleton<FailureTestingService>();
            services.AddScoped<IValidator<FailureTestingRequest>, FailureTestingRequestValidator>();
            services.AddControllers().AddApplicationPart(typeof(FailureTestingController).Assembly);
        });
    }

    private sealed class EmptyVendorRepository : IVendorRepository
    {
        public Task<Vendor?> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Vendor?>(null);
        }

        public Task<PagedResult<Vendor>> ListAsync(
            string? namePrefix,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<Vendor>([], 0));
        }

        public Task AddAsync(Vendor vendor, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
