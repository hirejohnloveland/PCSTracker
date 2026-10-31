using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;
using PartsTrackerErp.GoldenPattern.Core.Mapping;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;

public sealed class VendorsApplicationService(
    IVendorRepository vendorRepository,
    IUnitOfWork unitOfWork) : IVendorsApplicationService
{
    public async Task<ServiceResponse<VendorResponseDto>> AddAsync(string name, CancellationToken cancellationToken = default)
    {
        var vendor = Vendor.Create(name);
        await vendorRepository.AddAsync(vendor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResponse<VendorResponseDto>.Succeed(vendor.ToResponse());
    }

    public async Task<ServiceResponse<VendorResponseDto>> ActivateAsync(string id, CancellationToken cancellationToken = default)
    {
        Vendor? vendor = await vendorRepository.GetAsync(id, cancellationToken);
        if (vendor is null)
        {
            return ServiceResponse<VendorResponseDto>.Fail(ErrorType.NotFound, VendorErrors.NotFound(id));
        }

        vendor.Activate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResponse<VendorResponseDto>.Succeed(vendor.ToResponse());
    }

    public async Task<ServiceResponse<VendorResponseDto>> DeactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        Vendor? vendor = await vendorRepository.GetAsync(id, cancellationToken);
        if (vendor is null)
        {
            return ServiceResponse<VendorResponseDto>.Fail(ErrorType.NotFound, VendorErrors.NotFound(id));
        }

        vendor.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResponse<VendorResponseDto>.Succeed(vendor.ToResponse());
    }
}
