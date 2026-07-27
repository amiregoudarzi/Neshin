using FastEndpoints;
using DomainOrder = Neshin.Domain.Ordering.Order;

namespace Neshin.Api.Endpoints.Users.OrderHistory;

internal sealed record Query : ICommand<IReadOnlyList<Query.Response>>
{
    internal sealed record Response
    {
        public Guid OrderId { get; init; }
        public Guid CafeId { get; init; }
        public string CafeName { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public IReadOnlyList<ItemResponse> Items { get; init; } = [];
    }

    internal sealed record ItemResponse(Guid MenuItemId, string Title, decimal UnitPrice, int Quantity);

    private sealed class Handler(
        IUserExperienceRepository repository,
        IRequestContext requestContext) : ICommandHandler<Query, IReadOnlyList<Response>>
    {
        public async Task<IReadOnlyList<Response>> ExecuteAsync(Query query, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var orders = await repository.GetAcceptedOrderHistoryAsync(
                requestContext.CustomerSessionToken,
                ct);
            return orders.Select(result => ToResponse(result.Order, result.CafeName)).ToList();
        }

        private static Response ToResponse(DomainOrder order, string cafeName) => new()
        {
            OrderId = order.Id,
            CafeId = order.BranchId,
            CafeName = cafeName,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            CreatedAtUtc = order.CreatedAtUtc,
            Items = order.Items.Select(item =>
                new ItemResponse(item.MenuItemId, item.Name, item.UnitPrice, item.Quantity)).ToList()
        };
    }
}
