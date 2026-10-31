using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;

public sealed class VendorsApplicationService(
    IVendorRepository vendorRepository,
    IUnitOfWork unitOfWork) : IVendorsApplicationService
{
    public async Task<Vendor> AddAsync(string name, CancellationToken cancellationToken = default)
    {
        var vendor = Vendor.Create(name);
        await vendorRepository.AddAsync(vendor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return vendor;
    }

    public async Task<Vendor?> ActivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var vendor = await vendorRepository.GetAsync(id, cancellationToken);
        if (vendor is null)
            return null;

        vendor.Activate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return vendor;
    }

    public async Task<Vendor?> DeactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var vendor = await vendorRepository.GetAsync(id, cancellationToken);
        if (vendor is null)
            return null;

        vendor.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return vendor;
    }
}
