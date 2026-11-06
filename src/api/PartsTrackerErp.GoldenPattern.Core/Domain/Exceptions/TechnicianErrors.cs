namespace PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

public static class TechnicianErrors
{
    public static DomainError NotFound(string id)
    {
        return new()
        {
            Code = "TechnicianNotFound",
            Message = $"Technician '{id}' was not found."
        };
    }
}
