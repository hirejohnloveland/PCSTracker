using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;
using PartsTrackerErp.GoldenPattern.Core.Mapping;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;

public sealed class CustomersApplicationService(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork) : ICustomersApplicationService
{
    public async Task<ServiceResponse<CustomerResponseDto>> AddAsync(CustomerDto request, CancellationToken cancellationToken = default)
    {
        var customer = Customer.Create(
            request.Name,
            request.Description,
            request.Contact,
            request.Phone,
            request.Notes,
            request.QuickbooksId);
        await customerRepository.AddAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResponse<CustomerResponseDto>.Succeed(customer.ToResponseDto());
    }

    public async Task<ServiceResponse<CustomerResponseDto>> UpdateAsync(
        string id,
        CustomerDto request,
        CancellationToken cancellationToken = default)
    {
        Customer? customer = await customerRepository.GetAsync(id, cancellationToken);
        if (customer is null)
        {
            return ServiceResponse<CustomerResponseDto>.Fail(ErrorType.NotFound, CustomerErrors.NotFound(id));
        }

        customer.Update(
            request.Name,
            request.Description,
            request.Contact,
            request.Phone,
            request.Notes,
            request.QuickbooksId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResponse<CustomerResponseDto>.Succeed(customer.ToResponseDto());
    }

    public async Task<ServiceResponse<CustomerResponseDto>> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        Customer? customer = await customerRepository.GetAsync(id, cancellationToken);
        if (customer is null)
        {
            return ServiceResponse<CustomerResponseDto>.Fail(ErrorType.NotFound, CustomerErrors.NotFound(id));
        }

        return ServiceResponse<CustomerResponseDto>.Succeed(customer.ToResponseDto());
    }

    public async Task<ServiceResponse<PagedResponseDto<CustomerResponseDto>>> ListAsync(
        ListCustomersDto request,
        CancellationToken cancellationToken = default)
    {
        PagedResult<Customer> customers = await customerRepository.ListAsync(
            nameSearch: request.Name?.Trim(),
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        PagedResponseDto<CustomerResponseDto> customerPage = customers.ToPagedResponseDto(
            page: request.Page,
            pageSize: request.PageSize,
            map: customer => customer.ToResponseDto());

        return ServiceResponse<PagedResponseDto<CustomerResponseDto>>.Succeed(customerPage);
    }
}
