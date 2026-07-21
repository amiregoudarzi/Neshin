using Neshin.Domain.Common;
using Neshin.Domain.Ordering;

namespace Neshin.Domain.UnitTests.Ordering;

public sealed class OrderTests
{
    [Fact]
    public void Create_WhenPayAtVenueIsDisabled_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Order.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            PaymentMethod.PayAtVenuePos,
            branchAcceptsAppOrders: true,
            branchAllowsPayAtVenue: false,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void AddItem_SnapshotsPriceAndCalculatesTotal()
    {
        var order = Order.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            PaymentMethod.Online,
            branchAcceptsAppOrders: true,
            branchAllowsPayAtVenue: false,
            DateTimeOffset.UtcNow);

        order.AddItem(Guid.NewGuid(), "Latte", 180_000m, 2);

        Assert.Equal(360_000m, order.TotalAmount);
        Assert.Single(order.Items);
    }
}
