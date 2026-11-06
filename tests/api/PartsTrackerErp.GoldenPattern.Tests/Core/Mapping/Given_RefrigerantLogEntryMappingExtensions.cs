using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Mapping;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.Mapping;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_RefrigerantLogEntryMappingExtensions
{
    private static readonly DateOnly s_date = new(2026, 5, 15);

    [Fact]
    public void ToResponseDto_CopiesTheRequiredDetailsAndLeavesTheOptionalOnesNull()
    {
        RefrigerantLogEntry entry = RefrigerantLogEntry
            .Create(s_date, "Aldo Cipriani", "R-404A", 24.5m, "Leak repair")
            .WithId("refrigerantlogentries/1-A");

        RefrigerantLogEntryResponseDto response = entry.ToResponseDto();

        Assert.Equal(
            new RefrigerantLogEntryResponseDto(
                "refrigerantlogentries/1-A",
                s_date,
                "Aldo Cipriani",
                "R-404A",
                24.5m,
                "Leak repair",
                null,
                null,
                null,
                null),
            response);
    }

    [Fact]
    public void ToResponseDto_CopiesTheOptionalDetails()
    {
        RefrigerantLogEntry entry = RefrigerantLogEntry
            .Create(s_date, "Bob Osborne", "R-22", 6m, "Recharge", "EM-0412", "MFM-BX-01", "Metro Food Market - Bronx", "PO-2026-0081")
            .WithId("refrigerantlogentries/2-A");

        RefrigerantLogEntryResponseDto response = entry.ToResponseDto();

        Assert.Equal(
            new RefrigerantLogEntryResponseDto(
                "refrigerantlogentries/2-A",
                s_date,
                "Bob Osborne",
                "R-22",
                6m,
                "Recharge",
                "EM-0412",
                "MFM-BX-01",
                "Metro Food Market - Bronx",
                "PO-2026-0081"),
            response);
    }

    [Fact]
    public void ToResponseDto_EntryNotYetStored_Throws()
    {
        RefrigerantLogEntry entry = RefrigerantLogEntry.Create(s_date, "Bob Osborne", "R-22", 6m, "Recharge");

        Assert.Throws<InvalidOperationException>(entry.ToResponseDto);
    }
}
