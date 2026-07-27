using Neshin.Domain.Common;

namespace Neshin.Domain.Ordering;

public sealed class Invoice : AggregateRoot
{
    private readonly List<InvoiceItem> _items = [];

    private Invoice(Guid id, Guid branchId, Guid customerId, DateTime createdAtUtc)
        : base(id)
    {
        BranchId = branchId;
        CustomerId = customerId;
        CreatedAtUtc = createdAtUtc;
        Status = InvoiceStatus.Pending;
    }

    private Invoice() : base(Guid.Empty) { }

    public Guid BranchId { get; private init; }
    public Guid CustomerId { get; private init; }
    public InvoiceStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAtUtc { get; private init; }
    public DateTime? PaidAtUtc { get; private set; }
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    public static Invoice Create(Guid branchId, Guid customerId, DateTime createdAtUtc)
    {
        if (branchId == Guid.Empty || customerId == Guid.Empty)
            throw new DomainException("Cafe and customer are required.");
        return new Invoice(Guid.NewGuid(), branchId, customerId, createdAtUtc);
    }

    public void AddItem(Guid menuItemId, string title, decimal unitPrice, int quantity)
    {
        if (menuItemId == Guid.Empty) throw new DomainException("Menu item is required.");
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Item title is required.");
        if (unitPrice < 0) throw new DomainException("Unit price cannot be negative.");
        if (quantity is < 1 or > 20) throw new DomainException("Quantity must be between 1 and 20.");

        _items.Add(InvoiceItem.Create(Id, menuItemId, title.Trim(), unitPrice, quantity, CreatedAtUtc));
        TotalAmount = _items.Sum(item => item.UnitPrice * item.Quantity);
    }

    public void MarkPaid(DateTime now)
    {
        if (Status != InvoiceStatus.Pending) throw new DomainException("Invoice is not pending.");
        Status = InvoiceStatus.Paid;
        PaidAtUtc = now;
    }
}

public enum InvoiceStatus
{
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Cancelled = 4
}

public sealed class InvoiceItem : Entity
{
    private InvoiceItem(
        Guid id,
        Guid invoiceId,
        Guid menuItemId,
        string title,
        decimal unitPrice,
        int quantity,
        DateTime createdAtUtc) : base(id)
    {
        InvoiceId = invoiceId;
        MenuItemId = menuItemId;
        Title = title;
        UnitPrice = unitPrice;
        Quantity = quantity;
        CreatedAtUtc = createdAtUtc;
    }

    private InvoiceItem() : base(Guid.Empty) { }

    public Guid InvoiceId { get; private init; }
    public Guid MenuItemId { get; private init; }
    public string Title { get; private init; } = string.Empty;
    public decimal UnitPrice { get; private init; }
    public int Quantity { get; private init; }
    public DateTime CreatedAtUtc { get; private init; }

    internal static InvoiceItem Create(
        Guid invoiceId,
        Guid menuItemId,
        string title,
        decimal unitPrice,
        int quantity,
        DateTime createdAtUtc) =>
        new(Guid.NewGuid(), invoiceId, menuItemId, title, unitPrice, quantity, createdAtUtc);
}
