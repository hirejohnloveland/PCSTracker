namespace PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

public static class QuickbooksErrors
{
    public static DomainError SyncNotImplemented()
    {
        return new()
        {
            Code = "QuickbooksSyncNotImplemented",
            Message = "This feature is not implemented yet."
        };
    }
}
