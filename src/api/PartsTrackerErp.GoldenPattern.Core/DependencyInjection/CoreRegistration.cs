using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;
using PartsTrackerErp.GoldenPattern.Core.Infrastructure.Persistence;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Implementations;
using PartsTrackerErp.GoldenPattern.Core.Validation.Validators;

namespace PartsTrackerErp.GoldenPattern.Core.DependencyInjection;

public static class CoreRegistration
{
    /// <summary>
    /// Registers the Core business services. Called by every host (API, future Functions hosts).
    /// </summary>
    public static IHostApplicationBuilder AddCore(this IHostApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssemblyContaining<VendorInvoiceDtoValidator>();
        builder.AddCoreRavenDb();

        builder.Services.AddScoped<IVendorRepository, VendorRepository>();
        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
        builder.Services.AddScoped<IUnitOfWork, RavenUnitOfWork>();
        builder.Services.AddScoped<IVendorsApplicationService, VendorsApplicationService>();
        builder.Services.AddScoped<ICustomersApplicationService, CustomersApplicationService>();
        builder.Services.AddScoped<IVendorInvoicesApplicationService, VendorInvoicesApplicationService>();
        builder.Services.AddScoped<IQuickbooksApplicationService, QuickbooksApplicationService>();

        return builder;
    }
}
