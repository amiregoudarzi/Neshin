using FastEndpoints;
using DomainOrder = Neshin.Domain.Ordering.Order;

namespace Neshin.Api.Endpoints.CustomerOrders.Create;

internal sealed record Command : ICommand<Command.Response>
{
    public Guid BranchId { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public IReadOnlyList<ItemRequest> Items { get; init; } = [];
    public string? DisplayName { get; init; }
    public string? ContactPhoneNumber { get; init; }
    public bool AllowPhoneContact { get; init; }
    private sealed class Handler(
        ICustomerOrderRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Response> ExecuteAsync(Command command, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var order = await repository.PlaceOrderAsync(
                requestContext.CustomerSessionToken,
                command.BranchId,
                requestContext.IdempotencyKey,
                command.PaymentMethod,
                command.Items.Select(item => (item.MenuItemId, item.Quantity)).ToList(),
                command.DisplayName,
                command.ContactPhoneNumber,
                command.AllowPhoneContact,
                ct);
            return ToResponse(order);
        }

        private static Response ToResponse(DomainOrder order) => new()
        {
            OrderId = order.Id,
            BranchId = order.BranchId,
            Status = order.Status.ToString(),
            PaymentMethod = order.PaymentMethod.ToString(),
            TotalAmount = order.TotalAmount,
            Version = order.Version,
            CreatedAtUtc = order.CreatedAtUtc,
            RejectionReason = order.RejectionReason,
            Items = order.Items.Select(item => new ItemResponse
            {
                MenuItemId = item.MenuItemId,
                Name = item.Name,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList()
        };
    }

    internal sealed record ItemRequest
    {
        public Guid MenuItemId { get; init; }
        public int Quantity { get; init; }
    }

    internal sealed record Response
    {
        public Guid OrderId { get; init; }
        public Guid BranchId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string PaymentMethod { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public int Version { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public string? RejectionReason { get; init; }
        public IReadOnlyList<ItemResponse> Items { get; init; } = [];
    }

    internal sealed record ItemResponse
    {
        public Guid MenuItemId { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }
        public int Quantity { get; init; }
    }
}
