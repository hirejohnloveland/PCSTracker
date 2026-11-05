using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
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
        return ServiceResponse<VendorResponseDto>.Succeed(vendor.ToResponseDto());
    }

    public async Task<ServiceResponse<VendorResponseDto>> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        Vendor? vendor = await vendorRepository.GetAsync(id, cancellationToken);
        if (vendor is null)
        {
            return ServiceResponse<VendorResponseDto>.Fail(ErrorType.NotFound, VendorErrors.NotFound(id));
        }

        return ServiceResponse<VendorResponseDto>.Succeed(vendor.ToResponseDto());
    }

    public async Task<ServiceResponse<PagedResponseDto<VendorResponseDto>>> ListAsync(
        ListVendorsDto request,
        CancellationToken cancellationToken = default)
    {
        PagedResult<Vendor> vendors = await vendorRepository.ListAsync(
            nameSearch: request.Name?.Trim(),
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        PagedResponseDto<VendorResponseDto> vendorPage = vendors.ToPagedResponseDto(
            page: request.Page,
            pageSize: request.PageSize,
            map: vendor => vendor.ToResponseDto());

        return ServiceResponse<PagedResponseDto<VendorResponseDto>>.Succeed(vendorPage);
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
        return ServiceResponse<VendorResponseDto>.Succeed(vendor.ToResponseDto());
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
        return ServiceResponse<VendorResponseDto>.Succeed(vendor.ToResponseDto());
    }
}
