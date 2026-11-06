using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Mapping;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.Mapping;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_TechnicianMappingExtensions
{
    [Fact]
    public void ToResponseDto_CopiesTheIdFullNameRoleAndActive()
    {
        Technician technician = Technician.Create("Bob Osborne", "Service Tech").WithId("technicians-1-A");

        TechnicianResponseDto response = technician.ToResponseDto();

        Assert.Equal(new TechnicianResponseDto("technicians-1-A", "Bob Osborne", "Service Tech", true, null, null, null), response);
    }

    [Fact]
    public void ToResponseDto_CopiesThePhoneNotesQuickbooksIdAndAnInactiveState()
    {
        Technician technician = Technician
            .Create("Bob Osborne", "Installer", "214-309-2915", "Nights", "qb-42", active: false)
            .WithId("technicians-1-A");

        TechnicianResponseDto response = technician.ToResponseDto();

        Assert.Equal(
            new TechnicianResponseDto("technicians-1-A", "Bob Osborne", "Installer", false, "qb-42", "214-309-2915", "Nights"),
            response);
    }

    [Fact]
    public void ToResponseDto_TechnicianNotYetStored_Throws()
    {
        Technician technician = Technician.Create("Bob Osborne", "Service Tech");

        Assert.Throws<InvalidOperationException>(technician.ToResponseDto);
    }
}
