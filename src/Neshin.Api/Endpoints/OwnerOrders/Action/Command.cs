using FastEndpoints;
using DomainOrder = Neshin.Domain.Ordering.Order;

namespace Neshin.Api.Endpoints.OwnerOrders.Action;

internal sealed record Command : ICommand<Command.Response>
{
    public Guid BranchId { get; init; }
    public Guid OrderId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public int ExpectedVersion { get; init; }
    private sealed class Handler(
        IOwnerExperienceRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Response> ExecuteAsync(Command command, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var order = await repository.ChangeOrderStatusAsync(
                command.BranchId,
                command.OrderId,
                requestContext.ManagementKey,
                command.Action,
                command.Reason,
                command.ExpectedVersion,
                ct);
            return ToResponse(order);
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
