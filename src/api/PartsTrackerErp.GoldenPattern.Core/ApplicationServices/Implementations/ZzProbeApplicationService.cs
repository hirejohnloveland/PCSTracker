using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Repositories.Abstractions;

namespace PartsTrackerErp.GoldenPattern.Core.ApplicationServices.Implementations;

public sealed class ZzProbeApplicationService(IVendorRepository vendorRepository)
{
    public async Task<Vendor> RenameAsync(string id)
    {
        Vendor? vendor = await vendorRepository.GetAsync(id);
        if (vendor is null)
        {
            throw new InvalidOperationException("Vendor not found.");
        }

        return vendor;
    }
}
