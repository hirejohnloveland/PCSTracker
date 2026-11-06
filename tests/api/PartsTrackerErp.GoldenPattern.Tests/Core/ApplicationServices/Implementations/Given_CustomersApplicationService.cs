using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.ApplicationServices.Implementations;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_CustomersApplicationService
{
    private readonly Mock<ICustomerRepository> _repository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CustomersApplicationService _service;

    public Given_CustomersApplicationService()
    {
        _service = new CustomersApplicationService(_repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task AddAsync_StoresTheCustomerSavesAndSucceeds()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        _repository
            .Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .Callback<Customer, CancellationToken>((customer, _) => customer.WithId("customers-1-A"));

        ServiceResponse<CustomerResponseDto> response = await _service.AddAsync(
            new CustomerDto { Name = "  Acme  " },
            token);

        Assert.True(response.Success);
        Assert.Equal("customers-1-A", response.Data.Id);
        Assert.Equal("Acme", response.Data.Name);
        _repository.Verify(r => r.AddAsync(It.Is<Customer>(v => v.Name == "Acme"), token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(token), Times.Once);
    }

    [Fact]
    public async Task AddAsync_StoresTheTrimmedDescriptionContactPhoneNotesAndQuickbooksIdAndLeavesBlankOnesNull()
    {
        _repository
            .Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .Callback<Customer, CancellationToken>((customer, _) => customer.WithId("customers-1-A"));

        ServiceResponse<CustomerResponseDto> response = await _service.AddAsync(
            new CustomerDto
            {
                Name = "Acme",
                Description = "  Refrigeration parts  ",
                Contact = "Pat",
                Phone = "   ",
                Notes = null,
                QuickbooksId = "  58  "
            });

        Assert.True(response.Success);
        Assert.Equal(
            new CustomerResponseDto("customers-1-A", "Acme", "58", "Refrigeration parts", "Pat", null, null),
            response.Data);
    }

    [Fact]
    public async Task UpdateAsync_MissingCustomer_FailsWithNotFoundAndDoesNotSave()
    {
        _repository.Setup(r => r.GetAsync("7", It.IsAny<CancellationToken>())).ReturnsAsync((Customer?)null);

        ServiceResponse<CustomerResponseDto> response = await _service.UpdateAsync(
            "7",
            new CustomerDto { Name = "Acme" });

        AssertNotFound(response, "7");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExistingCustomer_ReplacesItsDetailsSavesAndSucceeds()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        Customer customer = Customer.Create("Acme", "Old", "Old contact", "555-0100", "Old notes", "58").WithId("customers-1-A");
        _repository.Setup(r => r.GetAsync("customers-1-A", token)).ReturnsAsync(customer);

        ServiceResponse<CustomerResponseDto> response = await _service.UpdateAsync(
            "customers-1-A",
            new CustomerDto
            {
                Name = "  Acme Supply  ",
                Description = "New",
                Contact = "   ",
                Phone = null,
                Notes = "New notes",
                QuickbooksId = "59"
            },
            token);

        Assert.True(response.Success);
        Assert.Equal(
            new CustomerResponseDto("customers-1-A", "Acme Supply", "59", "New", null, null, "New notes"),
            response.Data);
        _unitOfWork.Verify(u => u.SaveChangesAsync(token), Times.Once);
    }

    [Fact]
    public async Task GetAsync_MissingCustomer_FailsWithNotFoundAndDoesNotSave()
    {
        _repository.Setup(r => r.GetAsync("7", It.IsAny<CancellationToken>())).ReturnsAsync((Customer?)null);

        ServiceResponse<CustomerResponseDto> response = await _service.GetAsync("7");

        AssertNotFound(response, "7");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_ExistingCustomer_SucceedsWithItsDtoAndDoesNotSave()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        Customer customer = Customer.Create("Acme").WithId("customers-1-A");
        _repository.Setup(r => r.GetAsync("7", token)).ReturnsAsync(customer);

        ServiceResponse<CustomerResponseDto> response = await _service.GetAsync("7", token);

        Assert.True(response.Success);
        Assert.Equal(new CustomerResponseDto("customers-1-A", "Acme", null, null, null, null, null), response.Data);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListAsync_PassesTheTrimmedNamePageAndPageSizeAndTheTokenToTheRepository()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        SetupList(new PagedResult<Customer>([], 0));

        ServiceResponse<PagedResponseDto<CustomerResponseDto>> response = await _service.ListAsync(
            new ListCustomersDto { Name = "  Acme  ", Page = 3, PageSize = 10 },
            token);

        Assert.True(response.Success);
        _repository.Verify(r => r.ListAsync("Acme", 3, 10, token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListAsync_NoName_AsksTheRepositoryForNoNameFilter()
    {
        SetupList(new PagedResult<Customer>([], 0));

        await _service.ListAsync(new ListCustomersDto());

        _repository.Verify(r => r.ListAsync(null, 1, 25, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListAsync_MapsTheCustomersToDtosAndBuildsThePagination()
    {
        Customer acme = Customer.Create("Acme").WithId("customers-1-A");
        Customer beta = Customer.Create("Beta").WithId("customers-2-A");
        SetupList(new PagedResult<Customer>([acme, beta], 21));

        ServiceResponse<PagedResponseDto<CustomerResponseDto>> response = await _service.ListAsync(
            new ListCustomersDto { Page = 3, PageSize = 10 });

        Assert.True(response.Success);
        Assert.Equal(
            [new CustomerResponseDto("customers-1-A", "Acme", null, null, null, null, null), new CustomerResponseDto("customers-2-A", "Beta", null, null, null, null, null)],
            response.Data.Items);
        Assert.Equal(
            new PaginationMetadata(Page: 3, PageSize: 10, TotalCount: 21, TotalPages: 3),
            response.Data.Pagination);
    }

    private void SetupList(PagedResult<Customer> result)
    {
        _repository
            .Setup(r => r.ListAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    private static void AssertNotFound(ServiceResponse<CustomerResponseDto> response, string id)
    {
        Assert.False(response.Success);
        Assert.Equal(ErrorType.NotFound, response.FailureType);
        DomainError error = Assert.Single(response.Errors);
        Assert.Equal("CustomerNotFound", error.Code);
        Assert.Contains(id, error.Message);
    }
}
