using Neshin.Domain.Common;

namespace Neshin.Domain.Ordering;

public sealed class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    private Order(Guid id, Guid branchId, Guid userId, PaymentMethod paymentMethod, DateTimeOffset createdAtUtc)
        : base(id)
    {
        BranchId = branchId;
        UserId = userId;
        PaymentMethod = paymentMethod;
        CreatedAtUtc = createdAtUtc;
        Status = OrderStatus.Draft;
    }

    private Order() : base(Guid.Empty) { }

    public Guid BranchId { get; private init; }
    public Guid UserId { get; private init; }
    public PaymentMethod PaymentMethod { get; private init; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private init; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public static Order Create(
        Guid branchId,
        Guid userId,
        PaymentMethod paymentMethod,
        bool branchAcceptsAppOrders,
        bool branchAllowsPayAtVenue,
        DateTimeOffset createdAtUtc)
    {
        if (branchId == Guid.Empty || userId == Guid.Empty)
        {
            throw new DomainException("Branch and user are required.");
        }

        if (!branchAcceptsAppOrders)
        {
            throw new DomainException("This branch is not currently accepting app orders.");
        }

        if (paymentMethod == PaymentMethod.PayAtVenuePos && !branchAllowsPayAtVenue)
        {
            throw new DomainException("Pay at venue is not enabled for this branch.");
        }

        return new Order(Guid.NewGuid(), branchId, userId, paymentMethod, createdAtUtc);
    }

    public void AddItem(Guid menuItemId, string name, decimal unitPrice, int quantity)
    {
        if (menuItemId == Guid.Empty) throw new DomainException("Menu item is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Item name is required.");
        if (unitPrice < 0) throw new DomainException("Unit price cannot be negative.");
        if (quantity <= 0) throw new DomainException("Quantity must be positive.");

        _items.Add(OrderItem.Create(Id, menuItemId, name.Trim(), unitPrice, quantity));
        TotalAmount = _items.Sum(item => item.UnitPrice * item.Quantity);
    }

    public void SubmitForPayment()
    {
        if (_items.Count == 0) throw new DomainException("An empty order cannot be submitted.");
        if (Status != OrderStatus.Draft) throw new DomainException("Only a draft order can be submitted.");
        Status = OrderStatus.AwaitingPayment;
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.AwaitingPayment) throw new DomainException("Order is not awaiting payment.");
        Status = OrderStatus.Paid;
    }
}

public sealed class OrderItem : Entity
{
    private OrderItem(Guid id, Guid orderId, Guid menuItemId, string name, decimal unitPrice, int quantity)
        : base(id)
    {
        OrderId = orderId;
        MenuItemId = menuItemId;
        Name = name;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    private OrderItem() : base(Guid.Empty) { }

    public Guid OrderId { get; private init; }
    public Guid MenuItemId { get; private init; }
    public string Name { get; private init; } = string.Empty;
    public decimal UnitPrice { get; private init; }
    public int Quantity { get; private init; }

    internal static OrderItem Create(Guid orderId, Guid menuItemId, string name, decimal unitPrice, int quantity) =>
        new(Guid.NewGuid(), orderId, menuItemId, name, unitPrice, quantity);
}
