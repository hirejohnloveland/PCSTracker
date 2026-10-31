using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;

public sealed class VendorsApplicationService(
    IVendorRepository vendorRepository,
    IUnitOfWork unitOfWork) : IVendorsApplicationService
{
    public async Task<ServiceResponse<Vendor>> AddAsync(string name, CancellationToken cancellationToken = default)
    {
        var vendor = Vendor.Create(name);
        await vendorRepository.AddAsync(vendor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResponse<Vendor>.Succeed(vendor);
    }

    public async Task<ServiceResponse<Vendor>> ActivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var vendor = await vendorRepository.GetAsync(id, cancellationToken);
        if (vendor is null)
            return ServiceResponse<Vendor>.Fail(ErrorType.NotFound, VendorErrors.NotFound(id));

        vendor.Activate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResponse<Vendor>.Succeed(vendor);
    }

    public async Task<ServiceResponse<Vendor>> DeactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        var vendor = await vendorRepository.GetAsync(id, cancellationToken);
        if (vendor is null)
            return ServiceResponse<Vendor>.Fail(ErrorType.NotFound, VendorErrors.NotFound(id));

        vendor.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResponse<Vendor>.Succeed(vendor);
    }
}
