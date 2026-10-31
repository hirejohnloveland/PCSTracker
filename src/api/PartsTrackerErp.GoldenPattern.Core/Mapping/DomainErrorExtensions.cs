using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

namespace PartsTrackerErp.GoldenPattern.Core.Mapping;

public static class DomainErrorExtensions
{
    public static ErrorResponseItem ToResponseItem(this DomainError error) =>
        new(error.Code, error.Message);
}
