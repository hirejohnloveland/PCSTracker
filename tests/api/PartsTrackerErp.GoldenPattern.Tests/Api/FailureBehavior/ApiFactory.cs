using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.FailureBehavior;

/// <summary>
/// Boots the real API pipeline in memory, without a RavenDB server. Adds <see cref="FailureTestingController"/>.
/// Share one per test run, through <see cref="ApiPipelineCollection"/>.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public FailureTestingService TestingService => Services.GetRequiredService<FailureTestingService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IVendorRepository>();
            services.RemoveAll<ICustomerRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IVendorRepository, EmptyVendorRepository>();
            services.AddScoped<ICustomerRepository, EmptyCustomerRepository>();
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
            string? nameSearch,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<Vendor>([], 0));
        }

        public Task AddAsync(Vendor vendor, CancellationToken cancellationToken = default)
        {
            vendor.WithId("vendors-1-A");

            return Task.CompletedTask;
        }
    }

    private sealed class EmptyCustomerRepository : ICustomerRepository
    {
        public Task<Customer?> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Customer?>(null);
        }

        public Task<PagedResult<Customer>> ListAsync(
            string? nameSearch,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<Customer>([], 0));
        }

        public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            customer.WithId("customers-1-A");

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
