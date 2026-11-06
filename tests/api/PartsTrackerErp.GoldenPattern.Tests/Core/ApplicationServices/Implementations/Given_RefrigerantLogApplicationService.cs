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
public class Given_RefrigerantLogApplicationService
{
    private static readonly DateOnly s_date = new(2026, 5, 15);

    private readonly Mock<IRefrigerantLogRepository> _repository = new();
    private readonly Mock<ISettingsRepository> _settings = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RefrigerantLogApplicationService _service;

    public Given_RefrigerantLogApplicationService()
    {
        _service = new RefrigerantLogApplicationService(_repository.Object, _settings.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task AddAsync_StoresTheEntrySavesAndSucceeds()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        _repository
            .Setup(r => r.AddAsync(It.IsAny<RefrigerantLogEntry>(), It.IsAny<CancellationToken>()))
            .Callback<RefrigerantLogEntry, CancellationToken>((entry, _) => entry.WithId("refrigerantlogentries/1-A"));

        ServiceResponse<RefrigerantLogEntryResponseDto> response = await _service.AddAsync(
            new RefrigerantLogEntryDto
            {
                Date = s_date,
                Technician = "  Aldo Cipriani  ",
                RefrigerantType = "R-404A",
                AmountLbs = 24m,
                Notes = "Leak repair",
                TicketNumber = " EM-0415 ",
                Customer = "Atlantic Cold Storage - Brooklyn"
            },
            token);

        Assert.True(response.Success);
        Assert.Equal(
            new RefrigerantLogEntryResponseDto(
                "refrigerantlogentries/1-A",
                s_date,
                "Aldo Cipriani",
                "R-404A",
                24m,
                "Leak repair",
                "EM-0415",
                null,
                "Atlantic Cold Storage - Brooklyn",
                null),
            response.Data);
        _repository.Verify(r => r.AddAsync(It.IsAny<RefrigerantLogEntry>(), token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(token), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_MissingEntry_FailsWithNotFoundAndDoesNotSave()
    {
        _repository.Setup(r => r.GetAsync("7", It.IsAny<CancellationToken>())).ReturnsAsync((RefrigerantLogEntry?)null);

        ServiceResponse<RefrigerantLogEntryResponseDto> response = await _service.UpdateAsync("7", NewRequest());

        AssertNotFound(response, "7");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExistingEntry_ReplacesItsDetailsSavesAndSucceeds()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        RefrigerantLogEntry entry = RefrigerantLogEntry
            .Create(s_date, "Aldo Cipriani", "R-404A", 24m, "Old notes", "EM-0415")
            .WithId("refrigerantlogentries/1-A");
        _repository.Setup(r => r.GetAsync("refrigerantlogentries/1-A", token)).ReturnsAsync(entry);

        ServiceResponse<RefrigerantLogEntryResponseDto> response = await _service.UpdateAsync(
            "refrigerantlogentries/1-A",
            new RefrigerantLogEntryDto
            {
                Date = s_date.AddDays(1),
                Technician = "Bob Osborne",
                RefrigerantType = "R-22",
                AmountLbs = 6.5m,
                Notes = "New notes"
            },
            token);

        Assert.True(response.Success);
        Assert.Equal(
            new RefrigerantLogEntryResponseDto(
                "refrigerantlogentries/1-A",
                s_date.AddDays(1),
                "Bob Osborne",
                "R-22",
                6.5m,
                "New notes",
                null,
                null,
                null,
                null),
            response.Data);
        _unitOfWork.Verify(u => u.SaveChangesAsync(token), Times.Once);
    }

    [Fact]
    public async Task GetAsync_MissingEntry_FailsWithNotFoundAndDoesNotSave()
    {
        _repository.Setup(r => r.GetAsync("7", It.IsAny<CancellationToken>())).ReturnsAsync((RefrigerantLogEntry?)null);

        ServiceResponse<RefrigerantLogEntryResponseDto> response = await _service.GetAsync("7");

        AssertNotFound(response, "7");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_ExistingEntry_SucceedsWithItsDtoAndDoesNotSave()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        RefrigerantLogEntry entry = RefrigerantLogEntry
            .Create(s_date, "Bob Osborne", "R-22", 6m, "Recharge")
            .WithId("refrigerantlogentries/1-A");
        _repository.Setup(r => r.GetAsync("7", token)).ReturnsAsync(entry);

        ServiceResponse<RefrigerantLogEntryResponseDto> response = await _service.GetAsync("7", token);

        Assert.True(response.Success);
        Assert.Equal("refrigerantlogentries/1-A", response.Data.Id);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListAsync_PassesTheTrimmedFiltersPageAndPageSizeAndTheTokenToTheRepository()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        SetupList(new PagedResult<RefrigerantLogEntry>([], 0));

        ServiceResponse<PagedResponseDto<RefrigerantLogEntryResponseDto>> response = await _service.ListAsync(
            new ListRefrigerantLogEntriesDto
            {
                DateFrom = s_date,
                DateTo = s_date.AddDays(7),
                Technician = "  Bob Osborne  ",
                Customer = "  Metro  ",
                Page = 3,
                PageSize = 10
            },
            token);

        Assert.True(response.Success);
        _repository.Verify(
            r => r.ListAsync(s_date, s_date.AddDays(7), "Bob Osborne", "Metro", 3, 10, token),
            Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListAsync_NoFilters_AsksTheRepositoryForNoFilters()
    {
        SetupList(new PagedResult<RefrigerantLogEntry>([], 0));

        await _service.ListAsync(new ListRefrigerantLogEntriesDto());

        _repository.Verify(
            r => r.ListAsync(null, null, null, null, 1, 25, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ListAsync_MapsTheEntriesToDtosAndBuildsThePagination()
    {
        RefrigerantLogEntry first = RefrigerantLogEntry.Create(s_date, "Bob Osborne", "R-22", 6m, "Recharge").WithId("refrigerantlogentries/1-A");
        RefrigerantLogEntry second = RefrigerantLogEntry.Create(s_date, "Aldo Cipriani", "R-404A", 24m, "Leak").WithId("refrigerantlogentries/2-A");
        SetupList(new PagedResult<RefrigerantLogEntry>([first, second], 21));

        ServiceResponse<PagedResponseDto<RefrigerantLogEntryResponseDto>> response = await _service.ListAsync(
            new ListRefrigerantLogEntriesDto { Page = 3, PageSize = 10 });

        Assert.True(response.Success);
        Assert.Equal(["refrigerantlogentries/1-A", "refrigerantlogentries/2-A"], response.Data.Items.Select(i => i.Id));
        Assert.Equal(
            new PaginationMetadata(Page: 3, PageSize: 10, TotalCount: 21, TotalPages: 3),
            response.Data.Pagination);
    }

    [Fact]
    public async Task GetRefrigerantTypesAsync_ReturnsTheTypesInTheRefrigerantSettings()
    {
        using var cancellation = new CancellationTokenSource();
        CancellationToken token = cancellation.Token;
        var settings = new RefrigerantSettings();
        settings.Update(["R-22", "R-410A"]);
        _settings.Setup(s => s.GetAsync<RefrigerantSettings>(token)).ReturnsAsync(settings);

        ServiceResponse<RefrigerantTypesResponseDto> response = await _service.GetRefrigerantTypesAsync(token);

        Assert.True(response.Success);
        Assert.Equal(["R-22", "R-410A"], response.Data.Refrigerants);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static RefrigerantLogEntryDto NewRequest()
    {
        return new RefrigerantLogEntryDto
        {
            Date = s_date,
            Technician = "Bob Osborne",
            RefrigerantType = "R-22",
            AmountLbs = 6m,
            Notes = "Recharge"
        };
    }

    private void SetupList(PagedResult<RefrigerantLogEntry> result)
    {
        _repository
            .Setup(r => r.ListAsync(
                It.IsAny<DateOnly?>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    private static void AssertNotFound(ServiceResponse<RefrigerantLogEntryResponseDto> response, string id)
    {
        Assert.False(response.Success);
        Assert.Equal(ErrorType.NotFound, response.FailureType);
        DomainError error = Assert.Single(response.Errors);
        Assert.Equal("RefrigerantLogEntryNotFound", error.Code);
        Assert.Contains(id, error.Message);
    }
}
