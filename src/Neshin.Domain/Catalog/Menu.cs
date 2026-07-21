using Neshin.Domain.Common;

namespace Neshin.Domain.Catalog;

public sealed class Menu : AggregateRoot
{
    private Menu(Guid id, Guid branchId, string name) : base(id)
    {
        BranchId = branchId;
        Name = name;
    }

    private Menu() : base(Guid.Empty) { }

    public Guid BranchId { get; private init; }
    public string Name { get; private set; } = string.Empty;
    public bool IsPublished { get; private set; }

    public static Menu Create(Guid branchId, string name)
    {
        if (branchId == Guid.Empty) throw new DomainException("Branch is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Menu name is required.");
        return new Menu(Guid.NewGuid(), branchId, name.Trim());
    }

    public void Publish() => IsPublished = true;
    public void Unpublish() => IsPublished = false;
}
