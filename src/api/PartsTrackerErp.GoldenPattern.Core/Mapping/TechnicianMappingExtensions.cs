using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Core.Mapping;

public static class TechnicianMappingExtensions
{
    public static TechnicianResponseDto ToResponseDto(this Technician technician)
    {
        string id = technician.Id
            ?? throw new InvalidOperationException("A technician has no id until it is stored, and a response describes a stored technician.");

        return new TechnicianResponseDto(
            Id: id,
            FullName: technician.FullName,
            Role: technician.Role,
            Active: technician.Active,
            QuickbooksId: technician.QuickbooksId,
            Phone: technician.Phone,
            Notes: technician.Notes);
    }
}
