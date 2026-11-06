namespace PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

public static class CustomerErrors
{
    public static DomainError NotFound(string id)
    {
        return new()
        {
            Code = "CustomerNotFound",
            Message = $"Customer '{id}' was not found."
        };
    }
}
