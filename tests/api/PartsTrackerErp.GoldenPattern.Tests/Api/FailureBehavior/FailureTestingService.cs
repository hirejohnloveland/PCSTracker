using PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.FailureBehavior;

/// <summary>
/// Stands in for an application service. A test decides what the next call does: succeed, fail, or throw.
/// </summary>
public sealed class FailureTestingService
{
    private Func<ServiceResponse<string>> behavior = Success;

    public int Calls { get; private set; }

    public void Reset()
    {
        behavior = Success;
        Calls = 0;
    }

    public void Respond(ServiceResponse<string> response) => behavior = () => response;

    public void Throw(Exception exception) => behavior = () => throw exception;

    public async Task<ServiceResponse<string>> RunAsync(CancellationToken cancellationToken)
    {
        Calls++;

        // Fail after an await, as a real service does when its I/O fails.
        await Task.Yield();
        return behavior();
    }

    private static ServiceResponse<string> Success() => ServiceResponse<string>.Succeed("ok");
}
