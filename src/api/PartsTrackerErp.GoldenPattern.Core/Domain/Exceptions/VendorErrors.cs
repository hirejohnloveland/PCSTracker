namespace PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

public static class VendorErrors
{
    public static DomainError NotFound(string id)
    {
        return new()
        {
            Code = "VendorNotFound",
            Message = $"Vendor '{id}' was not found."
        };
    }
}
