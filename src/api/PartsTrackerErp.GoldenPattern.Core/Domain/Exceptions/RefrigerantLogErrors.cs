namespace PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

public static class RefrigerantLogErrors
{
    public static DomainError NotFound(string id)
    {
        return new()
        {
            Code = "RefrigerantLogEntryNotFound",
            Message = $"Refrigerant log entry '{id}' was not found."
        };
    }
}
