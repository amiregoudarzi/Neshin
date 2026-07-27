using Neshin.Domain.Clients;
using Neshin.Domain.Common;

namespace Neshin.Domain.UnitTests.Clients;

public sealed class BranchTests
{
    [Fact]
    public void SetAppOrdering_WhenBranchIsInactive_ThrowsDomainException()
    {
        var branch = Branch.Create(Guid.NewGuid(), "Central", 35.7219m, 51.3347m, DateTime.UtcNow);

        Assert.Throws<DomainException>(() => branch.SetAppOrdering(true));
    }

    [Fact]
    public void SetAppOrdering_WhenBranchIsActive_EnablesOrdering()
    {
        var branch = Branch.Create(Guid.NewGuid(), "Central", 35.7219m, 51.3347m, DateTime.UtcNow);
        branch.Activate();

        branch.SetAppOrdering(true);

        Assert.True(branch.AcceptsAppOrders);
    }
}
