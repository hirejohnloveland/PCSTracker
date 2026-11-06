using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities.Settings;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;
using PartsTrackerErp.GoldenPattern.Core.Mapping;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;

public sealed class RefrigerantLogApplicationService(
    IRefrigerantLogRepository refrigerantLogRepository,
    ISettingsRepository settingsRepository,
    IUnitOfWork unitOfWork) : IRefrigerantLogApplicationService
{
    public async Task<ServiceResponse<RefrigerantLogEntryResponseDto>> AddAsync(
        RefrigerantLogEntryDto request,
        CancellationToken cancellationToken = default)
    {
        var entry = RefrigerantLogEntry.Create(
            request.Date,
            request.Technician,
            request.RefrigerantType,
            request.AmountLbs,
            request.Notes,
            request.TicketNumber,
            request.JobName,
            request.Customer,
            request.VendorInvoiceRef);
        await refrigerantLogRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResponse<RefrigerantLogEntryResponseDto>.Succeed(entry.ToResponseDto());
    }

    public async Task<ServiceResponse<RefrigerantLogEntryResponseDto>> UpdateAsync(
        string id,
        RefrigerantLogEntryDto request,
        CancellationToken cancellationToken = default)
    {
        RefrigerantLogEntry? entry = await refrigerantLogRepository.GetAsync(id, cancellationToken);
        if (entry is null)
        {
            return ServiceResponse<RefrigerantLogEntryResponseDto>.Fail(ErrorType.NotFound, RefrigerantLogErrors.NotFound(id));
        }

        entry.Update(
            request.Date,
            request.Technician,
            request.RefrigerantType,
            request.AmountLbs,
            request.Notes,
            request.TicketNumber,
            request.JobName,
            request.Customer,
            request.VendorInvoiceRef);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResponse<RefrigerantLogEntryResponseDto>.Succeed(entry.ToResponseDto());
    }

    public async Task<ServiceResponse<RefrigerantLogEntryResponseDto>> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        RefrigerantLogEntry? entry = await refrigerantLogRepository.GetAsync(id, cancellationToken);
        if (entry is null)
        {
            return ServiceResponse<RefrigerantLogEntryResponseDto>.Fail(ErrorType.NotFound, RefrigerantLogErrors.NotFound(id));
        }

        return ServiceResponse<RefrigerantLogEntryResponseDto>.Succeed(entry.ToResponseDto());
    }

    public async Task<ServiceResponse<PagedResponseDto<RefrigerantLogEntryResponseDto>>> ListAsync(
        ListRefrigerantLogEntriesDto request,
        CancellationToken cancellationToken = default)
    {
        PagedResult<RefrigerantLogEntry> entries = await refrigerantLogRepository.ListAsync(
            dateFrom: request.DateFrom,
            dateTo: request.DateTo,
            technician: request.Technician?.Trim(),
            customerSearch: request.Customer?.Trim(),
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        PagedResponseDto<RefrigerantLogEntryResponseDto> entryPage = entries.ToPagedResponseDto(
            page: request.Page,
            pageSize: request.PageSize,
            map: entry => entry.ToResponseDto());

        return ServiceResponse<PagedResponseDto<RefrigerantLogEntryResponseDto>>.Succeed(entryPage);
    }

    public async Task<ServiceResponse<RefrigerantTypesResponseDto>> GetRefrigerantTypesAsync(
        CancellationToken cancellationToken = default)
    {
        RefrigerantSettings settings = await settingsRepository.GetAsync<RefrigerantSettings>(cancellationToken);

        return ServiceResponse<RefrigerantTypesResponseDto>.Succeed(new RefrigerantTypesResponseDto(settings.Refrigerants));
    }
}
