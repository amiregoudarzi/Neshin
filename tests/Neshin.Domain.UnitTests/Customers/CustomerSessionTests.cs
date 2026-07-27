using Neshin.Domain.Customers;

namespace Neshin.Domain.UnitTests.Customers;

public sealed class CustomerSessionTests
{
    [Fact]
    public void IsValidAt_AfterExpiry_ReturnsFalse()
    {
        var now = DateTime.UtcNow;
        var session = CustomerSession.Create(Guid.NewGuid(), "hash", now, now.AddMinutes(1));

        Assert.False(session.IsValidAt(now.AddMinutes(2)));
    }

    [Fact]
    public void Revoke_InvalidatesSessionImmediately()
    {
        var now = DateTime.UtcNow;
        var session = CustomerSession.Create(Guid.NewGuid(), "hash", now, now.AddDays(1));

        session.Revoke(now.AddMinutes(1));

        Assert.False(session.IsValidAt(now.AddMinutes(2)));
    }
}
