namespace PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

public static class VendorErrors
{
    public static DomainError NotFound(string id) => new()
    {
        Code = "VendorNotFound",
        Message = $"Vendor '{id}' was not found."
    };
}
