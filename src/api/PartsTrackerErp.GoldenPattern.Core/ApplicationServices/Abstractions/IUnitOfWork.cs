namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
