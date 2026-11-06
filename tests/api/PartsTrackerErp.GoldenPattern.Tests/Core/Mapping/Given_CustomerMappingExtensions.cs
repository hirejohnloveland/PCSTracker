using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;
using PartsTrackerErp.GoldenPattern.Core.Mapping;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.Mapping;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_CustomerMappingExtensions
{
    [Fact]
    public void ToResponseDto_CopiesTheIdAndName()
    {
        Customer customer = Customer.Create("Acme").WithId("customers-1-A");

        CustomerResponseDto response = customer.ToResponseDto();

        Assert.Equal(new CustomerResponseDto("customers-1-A", "Acme", null, null, null, null, null), response);
    }

    [Fact]
    public void ToResponseDto_CustomerLinkedToQuickbooks_CopiesTheQuickbooksId()
    {
        Customer customer = Customer.Create("Acme").WithId("customers-1-A").WithQuickbooksId("qb-42");

        CustomerResponseDto response = customer.ToResponseDto();

        Assert.Equal(new CustomerResponseDto("customers-1-A", "Acme", "qb-42", null, null, null, null), response);
    }

    [Fact]
    public void ToResponseDto_CopiesTheDescriptionContactPhoneAndNotes()
    {
        Customer customer = Customer.Create("Acme", "Parts", "Pat", "555-0100", "Net 30").WithId("customers-1-A");

        CustomerResponseDto response = customer.ToResponseDto();

        Assert.Equal(new CustomerResponseDto("customers-1-A", "Acme", null, "Parts", "Pat", "555-0100", "Net 30"), response);
    }

    [Fact]
    public void ToResponseDto_CustomerNotLinkedToQuickbooks_HasNoQuickbooksId()
    {
        Customer customer = Customer.Create("Acme").WithId("customers-1-A");

        CustomerResponseDto response = customer.ToResponseDto();

        Assert.Null(response.QuickbooksId);
    }

    [Fact]
    public void ToResponseDto_CustomerThatIsNotStored_Throws()
    {
        Customer customer = Customer.Create("Acme");

        Assert.Throws<InvalidOperationException>(() => customer.ToResponseDto());
    }
}
