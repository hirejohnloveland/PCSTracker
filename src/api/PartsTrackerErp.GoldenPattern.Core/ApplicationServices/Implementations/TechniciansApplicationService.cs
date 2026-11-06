using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Requests;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities.Settings;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;
using PartsTrackerErp.GoldenPattern.Core.Mapping;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;

public sealed class TechniciansApplicationService(
    ITechnicianRepository technicianRepository,
    ISettingsRepository settingsRepository,
    IUnitOfWork unitOfWork) : ITechniciansApplicationService
{
    public async Task<ServiceResponse<TechnicianResponseDto>> AddAsync(
        TechnicianDto request,
        CancellationToken cancellationToken = default)
    {
        var technician = Technician.Create(
            request.FullName,
            request.Role,
            request.Phone,
            request.Notes,
            request.QuickbooksId,
            request.Active);
        await technicianRepository.AddAsync(technician, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResponse<TechnicianResponseDto>.Succeed(technician.ToResponseDto());
    }

    public async Task<ServiceResponse<TechnicianResponseDto>> UpdateAsync(
        string id,
        TechnicianDto request,
        CancellationToken cancellationToken = default)
    {
        Technician? technician = await technicianRepository.GetAsync(id, cancellationToken);
        if (technician is null)
        {
            return ServiceResponse<TechnicianResponseDto>.Fail(ErrorType.NotFound, TechnicianErrors.NotFound(id));
        }

        technician.Update(
            request.FullName,
            request.Role,
            request.Phone,
            request.Notes,
            request.QuickbooksId,
            request.Active);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResponse<TechnicianResponseDto>.Succeed(technician.ToResponseDto());
    }

    public async Task<ServiceResponse<TechnicianResponseDto>> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        Technician? technician = await technicianRepository.GetAsync(id, cancellationToken);
        if (technician is null)
        {
            return ServiceResponse<TechnicianResponseDto>.Fail(ErrorType.NotFound, TechnicianErrors.NotFound(id));
        }

        return ServiceResponse<TechnicianResponseDto>.Succeed(technician.ToResponseDto());
    }

    public async Task<ServiceResponse<PagedResponseDto<TechnicianResponseDto>>> ListAsync(
        ListTechniciansDto request,
        CancellationToken cancellationToken = default)
    {
        PagedResult<Technician> technicians = await technicianRepository.ListAsync(
            nameSearch: request.Name?.Trim(),
            active: request.Active,
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        PagedResponseDto<TechnicianResponseDto> technicianPage = technicians.ToPagedResponseDto(
            page: request.Page,
            pageSize: request.PageSize,
            map: technician => technician.ToResponseDto());

        return ServiceResponse<PagedResponseDto<TechnicianResponseDto>>.Succeed(technicianPage);
    }

    public async Task<ServiceResponse<TechnicianRolesResponseDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        TechnicianRoleSettings settings = await settingsRepository.GetAsync<TechnicianRoleSettings>(cancellationToken);

        return ServiceResponse<TechnicianRolesResponseDto>.Succeed(new TechnicianRolesResponseDto(settings.Roles));
    }
}
