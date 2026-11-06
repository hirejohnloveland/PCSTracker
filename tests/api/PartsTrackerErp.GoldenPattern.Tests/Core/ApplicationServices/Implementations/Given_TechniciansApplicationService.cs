using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities.Settings;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.ApplicationServices.Implementations;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_TechniciansApplicationService
{
    private readonly Mock<ITechnicianRepository> _repository = new();
    private readonly Mock<ISettingsRepository> _settings = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly TechniciansApplicationService _service;

    public Given_TechniciansApplicationService()
    {
        _service = new TechniciansApplicationService(_repository.Object, _settings.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task AddAsync_StoresTheTechnicianSavesAndSucceeds()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        _repository
            .Setup(r => r.AddAsync(It.IsAny<Technician>(), It.IsAny<CancellationToken>()))
            .Callback<Technician, CancellationToken>((technician, _) => technician.WithId("technicians-1-A"));

        ServiceResponse<TechnicianResponseDto> response = await _service.AddAsync(
            new TechnicianDto { FullName = "  Bob Osborne  ", Role = " Service Tech " },
            token);

        Assert.True(response.Success);
        Assert.Equal(
            new TechnicianResponseDto("technicians-1-A", "Bob Osborne", "Service Tech", true, null, null, null),
            response.Data);
        _repository.Verify(r => r.AddAsync(It.Is<Technician>(t => t.FullName == "Bob Osborne"), token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(token), Times.Once);
    }

    [Fact]
    public async Task AddAsync_StoresTheTrimmedPhoneNotesAndQuickbooksIdAndLeavesBlankOnesNull()
    {
        _repository
            .Setup(r => r.AddAsync(It.IsAny<Technician>(), It.IsAny<CancellationToken>()))
            .Callback<Technician, CancellationToken>((technician, _) => technician.WithId("technicians-1-A"));

        ServiceResponse<TechnicianResponseDto> response = await _service.AddAsync(
            new TechnicianDto
            {
                FullName = "Bob Osborne",
                Role = "Installer",
                Active = false,
                Phone = "  214-309-2915  ",
                Notes = "   ",
                QuickbooksId = "  58  "
            });

        Assert.True(response.Success);
        Assert.Equal(
            new TechnicianResponseDto("technicians-1-A", "Bob Osborne", "Installer", false, "58", "214-309-2915", null),
            response.Data);
    }

    [Fact]
    public async Task UpdateAsync_MissingTechnician_FailsWithNotFoundAndDoesNotSave()
    {
        _repository.Setup(r => r.GetAsync("7", It.IsAny<CancellationToken>())).ReturnsAsync((Technician?)null);

        ServiceResponse<TechnicianResponseDto> response = await _service.UpdateAsync(
            "7",
            new TechnicianDto { FullName = "Bob Osborne", Role = "Service Tech" });

        AssertNotFound(response, "7");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTechnician_ReplacesItsDetailsSavesAndSucceeds()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        Technician technician = Technician
            .Create("Bob Osborne", "Service Tech", "214-309-2915", "Old notes", "58")
            .WithId("technicians-1-A");
        _repository.Setup(r => r.GetAsync("technicians-1-A", token)).ReturnsAsync(technician);

        ServiceResponse<TechnicianResponseDto> response = await _service.UpdateAsync(
            "technicians-1-A",
            new TechnicianDto
            {
                FullName = "  Robert Osborne  ",
                Role = "Installer",
                Active = false,
                Phone = null,
                Notes = "New notes",
                QuickbooksId = "59"
            },
            token);

        Assert.True(response.Success);
        Assert.Equal(
            new TechnicianResponseDto("technicians-1-A", "Robert Osborne", "Installer", false, "59", null, "New notes"),
            response.Data);
        _unitOfWork.Verify(u => u.SaveChangesAsync(token), Times.Once);
    }

    [Fact]
    public async Task GetAsync_MissingTechnician_FailsWithNotFoundAndDoesNotSave()
    {
        _repository.Setup(r => r.GetAsync("7", It.IsAny<CancellationToken>())).ReturnsAsync((Technician?)null);

        ServiceResponse<TechnicianResponseDto> response = await _service.GetAsync("7");

        AssertNotFound(response, "7");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_ExistingTechnician_SucceedsWithItsDtoAndDoesNotSave()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        Technician technician = Technician.Create("Bob Osborne", "Service Tech").WithId("technicians-1-A");
        _repository.Setup(r => r.GetAsync("7", token)).ReturnsAsync(technician);

        ServiceResponse<TechnicianResponseDto> response = await _service.GetAsync("7", token);

        Assert.True(response.Success);
        Assert.Equal(
            new TechnicianResponseDto("technicians-1-A", "Bob Osborne", "Service Tech", true, null, null, null),
            response.Data);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListAsync_PassesTheTrimmedNamePageAndPageSizeAndTheTokenToTheRepository()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        SetupList(new PagedResult<Technician>([], 0));

        ServiceResponse<PagedResponseDto<TechnicianResponseDto>> response = await _service.ListAsync(
            new ListTechniciansDto { Name = "  Bob  ", Page = 3, PageSize = 10 },
            token);

        Assert.True(response.Success);
        _repository.Verify(r => r.ListAsync("Bob", 3, 10, token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListAsync_NoName_AsksTheRepositoryForNoNameFilter()
    {
        SetupList(new PagedResult<Technician>([], 0));

        await _service.ListAsync(new ListTechniciansDto());

        _repository.Verify(r => r.ListAsync(null, 1, 25, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListAsync_MapsTheTechniciansToDtosAndBuildsThePagination()
    {
        Technician bob = Technician.Create("Bob Osborne", "Service Tech").WithId("technicians-1-A");
        Technician aldo = Technician.Create("Aldo Cipriani", "Installer", active: false).WithId("technicians-2-A");
        SetupList(new PagedResult<Technician>([bob, aldo], 21));

        ServiceResponse<PagedResponseDto<TechnicianResponseDto>> response = await _service.ListAsync(
            new ListTechniciansDto { Page = 3, PageSize = 10 });

        Assert.True(response.Success);
        Assert.Equal(
            [
                new TechnicianResponseDto("technicians-1-A", "Bob Osborne", "Service Tech", true, null, null, null),
                new TechnicianResponseDto("technicians-2-A", "Aldo Cipriani", "Installer", false, null, null, null)
            ],
            response.Data.Items);
        Assert.Equal(
            new PaginationMetadata(Page: 3, PageSize: 10, TotalCount: 21, TotalPages: 3),
            response.Data.Pagination);
    }

    [Fact]
    public async Task GetRolesAsync_ReturnsTheRolesInTheTechnicianRoleSettings()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        var settings = new TechnicianRoleSettings();
        settings.Update(["Lead", "Apprentice"]);
        _settings.Setup(s => s.GetAsync<TechnicianRoleSettings>(token)).ReturnsAsync(settings);

        ServiceResponse<TechnicianRolesResponseDto> response = await _service.GetRolesAsync(token);

        Assert.True(response.Success);
        Assert.Equal(["Lead", "Apprentice"], response.Data.Roles);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupList(PagedResult<Technician> result)
    {
        _repository
            .Setup(r => r.ListAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    private static void AssertNotFound(ServiceResponse<TechnicianResponseDto> response, string id)
    {
        Assert.False(response.Success);
        Assert.Equal(ErrorType.NotFound, response.FailureType);
        DomainError error = Assert.Single(response.Errors);
        Assert.Equal("TechnicianNotFound", error.Code);
        Assert.Contains(id, error.Message);
    }
}
