using FastEndpoints;
using DomainOrder = Neshin.Domain.Ordering.Order;

namespace Neshin.Api.Endpoints.OwnerOrders.List;

internal sealed record Query : ICommand<IReadOnlyList<Query.Response>>
{
    public Guid BranchId { get; init; }
    public string? Status { get; init; }
    private sealed class Handler(
        IOwnerExperienceRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Query, IReadOnlyList<Response>>
    {
        public async Task<IReadOnlyList<Response>> ExecuteAsync(Query query, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var orders = await repository.GetOrdersAsync(
                query.BranchId, requestContext.ManagementKey, query.Status, ct);
            return orders.Select(ToResponse).ToList();
        }

        private static Response ToResponse(DomainOrder order) => new()
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            PaymentMethod = order.PaymentMethod.ToString(),
            TotalAmount = order.TotalAmount,
            Version = order.Version,
            CreatedAtUtc = order.CreatedAtUtc,
            CustomerDisplayName = order.CustomerDisplayName,
            ContactPhoneNumber = order.AllowsPhoneContact ? order.ContactPhoneNumber : null,
            RejectionReason = order.RejectionReason,
            Items = order.Items.Select(item => new ItemResponse
            {
                Name = item.Name,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList()
        };
    }

    internal sealed record Response
    {
        public Guid OrderId { get; init; }
        public Guid CustomerId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string PaymentMethod { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public int Version { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public string? CustomerDisplayName { get; init; }
        public string? ContactPhoneNumber { get; init; }
        public string? RejectionReason { get; init; }
        public IReadOnlyList<ItemResponse> Items { get; init; } = [];
    }

    internal sealed record ItemResponse
    {
        public string Name { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }
        public int Quantity { get; init; }
    }
}
