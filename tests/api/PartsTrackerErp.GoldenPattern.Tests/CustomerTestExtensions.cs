using PartsTrackerErp.GoldenPattern.Core.Domain.Entities;

namespace PartsTrackerErp.GoldenPattern.Tests;

internal static class CustomerTestExtensions
{
    /// <summary>
    /// Gives a customer the id RavenDB assigns when it stores one. The setter is private, and the id stays null until
    /// the customer is stored.
    /// </summary>
    public static Customer WithId(this Customer customer, string id)
    {
        typeof(Customer).GetProperty(nameof(Customer.Id))!.SetValue(customer, id);

        return customer;
    }

    /// <summary>
    /// Gives an existing customer a QuickBooks id. The setter is private, and only <see cref="Customer.Create"/> sets it.
    /// </summary>
    public static Customer WithQuickbooksId(this Customer customer, string quickbooksId)
    {
        typeof(Customer).GetProperty(nameof(Customer.QuickbooksId))!.SetValue(customer, quickbooksId);

        return customer;
    }
}
