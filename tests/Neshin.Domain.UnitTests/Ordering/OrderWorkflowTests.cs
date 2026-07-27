using Neshin.Domain.Common;
using Neshin.Domain.Ordering;

namespace Neshin.Domain.UnitTests.Ordering;

public sealed class OrderWorkflowTests
{
    private static readonly DateTime Now = new(2026, 7, 21, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void SubmitForPayment_ForPayAtVenue_SubmitsDirectlyToVenue()
    {
        var order = Create(PaymentMethod.PayAtVenuePos);
        order.AddItem(Guid.NewGuid(), "Coffee", 100_000, 1);

        order.SubmitForPayment(Now);

        Assert.Equal(OrderStatus.Submitted, order.Status);
        Assert.Equal(Now, order.SubmittedAtUtc);
    }

    [Fact]
    public void SubmitForPayment_ForOnlinePayment_WaitsForPayment()
    {
        var order = Create(PaymentMethod.Online);
        order.AddItem(Guid.NewGuid(), "Coffee", 100_000, 1);

        order.SubmitForPayment(Now);

        Assert.Equal(OrderStatus.AwaitingPayment, order.Status);
    }

    [Fact]
    public void Workflow_AcceptPrepareReadyComplete_UsesGuardedTransitions()
    {
        var order = Create(PaymentMethod.PayAtVenuePos);
        order.AddItem(Guid.NewGuid(), "Coffee", 100_000, 1);
        order.SubmitForPayment(Now);

        order.Accept(Now.AddMinutes(1));
        order.StartPreparing();
        order.MarkReady(Now.AddMinutes(5));
        order.Complete(Now.AddMinutes(10));

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(6, order.Version);
    }

    [Fact]
    public void Reject_WithoutReason_ThrowsDomainException()
    {
        var order = Create(PaymentMethod.PayAtVenuePos);
        order.AddItem(Guid.NewGuid(), "Coffee", 100_000, 1);
        order.SubmitForPayment(Now);

        Assert.Throws<DomainException>(() => order.Reject(string.Empty, Now));
    }

    [Fact]
    public void ContactPhone_IsNotSharedWithoutExplicitConsent()
    {
        var order = Create(PaymentMethod.PayAtVenuePos);

        order.SetContact("Sara", "09120000000", allowPhoneContact: false);

        Assert.False(order.AllowsPhoneContact);
        Assert.Equal("09120000000", order.ContactPhoneNumber);
    }

    private static Order Create(PaymentMethod paymentMethod) => Order.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        paymentMethod,
        Guid.NewGuid().ToString("N"),
        branchAcceptsAppOrders: true,
        branchAllowsPayAtVenue: true,
        Now);
}
