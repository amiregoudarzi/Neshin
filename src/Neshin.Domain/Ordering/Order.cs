using Neshin.Domain.Common;

namespace Neshin.Domain.Ordering;

public sealed class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    private Order(
        Guid id,
        Guid branchId,
        Guid customerId,
        PaymentMethod paymentMethod,
        string idempotencyKey,
        DateTime createdAtUtc)
        : base(id)
    {
        BranchId = branchId;
        CustomerId = customerId;
        PaymentMethod = paymentMethod;
        IdempotencyKey = idempotencyKey;
        CreatedAtUtc = createdAtUtc;
        Status = OrderStatus.Draft;
        Version = 1;
    }

    private Order() : base(Guid.Empty) { }

    public Guid BranchId { get; private init; }
    public Guid CustomerId { get; private init; }
    public Guid? UserId { get; private init; }
    public PaymentMethod PaymentMethod { get; private init; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string IdempotencyKey { get; private init; } = string.Empty;
    public string? CustomerDisplayName { get; private set; }
    public string? ContactPhoneNumber { get; private set; }
    public bool AllowsPhoneContact { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime CreatedAtUtc { get; private init; }
    public DateTime? SubmittedAtUtc { get; private set; }
    public DateTime? AcceptedAtUtc { get; private set; }
    public DateTime? ReadyAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime? RejectedAtUtc { get; private set; }
    public int Version { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public static Order Create(
        Guid branchId,
        Guid customerId,
        PaymentMethod paymentMethod,
        string idempotencyKey,
        bool branchAcceptsAppOrders,
        bool branchAllowsPayAtVenue,
        DateTime createdAtUtc)
    {
        if (branchId == Guid.Empty || customerId == Guid.Empty)
        {
            throw new DomainException("Branch and customer are required.");
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
            throw new DomainException("A valid idempotency key is required.");

        if (!branchAcceptsAppOrders)
        {
            throw new DomainException("This branch is not currently accepting app orders.");
        }

        if (paymentMethod == PaymentMethod.PayAtVenuePos && !branchAllowsPayAtVenue)
        {
            throw new DomainException("Pay at venue is not enabled for this branch.");
        }

        return new Order(Guid.NewGuid(), branchId, customerId, paymentMethod, idempotencyKey.Trim(), createdAtUtc);
    }

    public void AddItem(Guid menuItemId, string name, decimal unitPrice, int quantity)
    {
        if (menuItemId == Guid.Empty) throw new DomainException("Menu item is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Item name is required.");
        if (unitPrice < 0) throw new DomainException("Unit price cannot be negative.");
        if (quantity <= 0) throw new DomainException("Quantity must be positive.");

        _items.Add(OrderItem.Create(Id, menuItemId, name.Trim(), unitPrice, quantity, CreatedAtUtc));
        TotalAmount = _items.Sum(item => item.UnitPrice * item.Quantity);
    }

    public void SubmitForPayment(DateTime now)
    {
        if (_items.Count == 0) throw new DomainException("An empty order cannot be submitted.");
        if (Status != OrderStatus.Draft) throw new DomainException("Only a draft order can be submitted.");
        Status = PaymentMethod == PaymentMethod.Online
            ? OrderStatus.AwaitingPayment
            : OrderStatus.Submitted;
        SubmittedAtUtc = now;
        Version++;
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.AwaitingPayment) throw new DomainException("Order is not awaiting payment.");
        Status = OrderStatus.Submitted;
        Version++;
    }

    public void SetContact(string? displayName, string? phoneNumber, bool allowPhoneContact)
    {
        CustomerDisplayName = NormalizeOptional(displayName, 100);
        ContactPhoneNumber = NormalizeOptional(phoneNumber, 30);
        AllowsPhoneContact = allowPhoneContact && ContactPhoneNumber is not null;
    }

    public void Accept(DateTime now)
    {
        EnsureStatus(OrderStatus.Submitted, "Only a submitted order can be accepted.");
        Status = OrderStatus.Accepted;
        AcceptedAtUtc = now;
        Version++;
    }

    public void Reject(string reason, DateTime now)
    {
        EnsureStatus(OrderStatus.Submitted, "Only a submitted order can be rejected.");
        if (string.IsNullOrWhiteSpace(reason)) throw new DomainException("A rejection reason is required.");
        if (reason.Length > 500) throw new DomainException("The rejection reason cannot exceed 500 characters.");

        Status = OrderStatus.Rejected;
        RejectionReason = reason.Trim();
        RejectedAtUtc = now;
        Version++;
    }

    public void StartPreparing()
    {
        EnsureStatus(OrderStatus.Accepted, "Only an accepted order can start preparation.");
        Status = OrderStatus.Preparing;
        Version++;
    }

    public void MarkReady(DateTime now)
    {
        EnsureStatus(OrderStatus.Preparing, "Only a preparing order can be marked ready.");
        Status = OrderStatus.Ready;
        ReadyAtUtc = now;
        Version++;
    }

    public void Complete(DateTime now)
    {
        EnsureStatus(OrderStatus.Ready, "Only a ready order can be completed.");
        Status = OrderStatus.Completed;
        CompletedAtUtc = now;
        Version++;
    }

    private void EnsureStatus(OrderStatus expected, string message)
    {
        if (Status != expected) throw new DomainException(message);
    }

    private static string? NormalizeOptional(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
            throw new DomainException($"The value cannot exceed {maximumLength} characters.");
        return normalized;
    }
}

public sealed class OrderItem : Entity
{
    private OrderItem(
        Guid id,
        Guid orderId,
        Guid menuItemId,
        string name,
        decimal unitPrice,
        int quantity,
        DateTime createdAtUtc)
        : base(id)
    {
        OrderId = orderId;
        MenuItemId = menuItemId;
        Name = name;
        UnitPrice = unitPrice;
        Quantity = quantity;
        CreatedAtUtc = createdAtUtc;
    }

    private OrderItem() : base(Guid.Empty) { }

    public Guid OrderId { get; private init; }
    public Guid MenuItemId { get; private init; }
    public string Name { get; private init; } = string.Empty;
    public decimal UnitPrice { get; private init; }
    public int Quantity { get; private init; }
    public DateTime CreatedAtUtc { get; private init; }

    internal static OrderItem Create(
        Guid orderId,
        Guid menuItemId,
        string name,
        decimal unitPrice,
        int quantity,
        DateTime createdAtUtc) =>
        new(Guid.NewGuid(), orderId, menuItemId, name, unitPrice, quantity, createdAtUtc);
}
