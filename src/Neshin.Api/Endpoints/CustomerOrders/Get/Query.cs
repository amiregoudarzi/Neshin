using FastEndpoints;
using DomainOrder = Neshin.Domain.Ordering.Order;

namespace Neshin.Api.Endpoints.CustomerOrders.Get;

internal sealed record Query : ICommand<Query.Response>
{
    public Guid OrderId { get; init; }
    private sealed class Handler(
        ICustomerOrderRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Query, Response>
    {
        public async Task<Response> ExecuteAsync(Query query, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var response = await repository.GetOrderAsync(
                requestContext.CustomerSessionToken,
                query.OrderId,
                ct);
            var order = response ?? throw new Neshin.Application.Common.ResourceNotFoundException(
                "The order was not found.");
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
