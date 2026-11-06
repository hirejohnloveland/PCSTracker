using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Core.Mapping;

public static class CustomerMappingExtensions
{
    public static CustomerResponseDto ToResponseDto(this Customer customer)
    {
        string id = customer.Id
            ?? throw new InvalidOperationException("A customer has no id until it is stored, and a response describes a stored customer.");

        return new CustomerResponseDto(
            Id: id,
            Name: customer.Name,
            QuickbooksId: customer.QuickbooksId,
            Description: customer.Description,
            Contact: customer.Contact,
            Phone: customer.Phone,
            Notes: customer.Notes);
    }
}
