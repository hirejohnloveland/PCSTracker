using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.Domain.Entities;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_RefrigerantLogEntry
{
    private static readonly DateOnly s_date = new(2026, 5, 15);

    [Fact]
    public void Create_TrimsTextAndStoresBlankOptionalDetailsAsNull()
    {
        RefrigerantLogEntry entry = RefrigerantLogEntry.Create(
            s_date,
            "  Aldo Cipriani  ",
            " R-404A ",
            24m,
            "  Leak repair  ",
            ticketNumber: "   ",
            jobName: "  ACS-BK-01  ",
            customerName: "",
            vendorInvoiceRef: null);

        Assert.Equal("Aldo Cipriani", entry.TechnicianName);
        Assert.Equal("R-404A", entry.RefrigerantType);
        Assert.Equal("Leak repair", entry.Notes);
        Assert.Null(entry.TicketNumber);
        Assert.Equal("ACS-BK-01", entry.JobName);
        Assert.Null(entry.CustomerName);
        Assert.Null(entry.VendorInvoiceRef);
    }

    [Fact]
    public void Update_ReplacesEveryDetailAndClearsBlankOptionalOnes()
    {
        RefrigerantLogEntry entry = RefrigerantLogEntry.Create(
            s_date, "Aldo Cipriani", "R-404A", 24m, "Leak repair", "EM-0415", "ACS-BK-01", "Atlantic Cold Storage", "URI-88430");

        entry.Update(s_date.AddDays(1), "Bob Osborne", "R-22", 6.5m, "Recharge", null, "MFM-BX-01", "", "PO-1");

        Assert.Equal(s_date.AddDays(1), entry.Date);
        Assert.Equal("Bob Osborne", entry.TechnicianName);
        Assert.Equal("R-22", entry.RefrigerantType);
        Assert.Equal(6.5m, entry.AmountLbs);
        Assert.Equal("Recharge", entry.Notes);
        Assert.Null(entry.TicketNumber);
        Assert.Equal("MFM-BX-01", entry.JobName);
        Assert.Null(entry.CustomerName);
        Assert.Equal("PO-1", entry.VendorInvoiceRef);
    }

    [Theory]
    [InlineData("", "R-22", 1, "Notes", "Technician is required.")]
    [InlineData("   ", "R-22", 1, "Notes", "Technician is required.")]
    [InlineData("Bob", "", 1, "Notes", "Refrigerant type is required.")]
    [InlineData("Bob", "R-22", 0, "Notes", "Amount used must be greater than zero.")]
    [InlineData("Bob", "R-22", -1, "Notes", "Amount used must be greater than zero.")]
    [InlineData("Bob", "R-22", 1, "  ", "Notes are required.")]
    public void Create_InvalidDetail_Throws(string technician, string refrigerant, int amount, string notes, string message)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => RefrigerantLogEntry.Create(s_date, technician, refrigerant, amount, notes));

        Assert.Equal(message, exception.Message);
    }
}
