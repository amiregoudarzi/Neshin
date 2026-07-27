using FastEndpoints;

namespace Neshin.Api.Endpoints.Clients.Sales.List;

internal sealed record Query : ICommand<IReadOnlyList<Query.Response>>
{
    public Guid CafeId { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid? MenuItemId { get; init; }

    internal sealed record Response
    {
        public Guid OrderId { get; init; }
        public Guid CustomerId { get; init; }
        public string Status { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public IReadOnlyList<ItemResponse> Items { get; init; } = [];
    }

    internal sealed record ItemResponse(
        Guid MenuItemId,
        string Title,
        decimal UnitPrice,
        int Quantity,
        decimal LineTotal);

    private sealed class Handler(
        IClientExperienceRepository repository,
        IRequestContext requestContext) : ICommandHandler<Query, IReadOnlyList<Response>>
    {
        public async Task<IReadOnlyList<Response>> ExecuteAsync(Query query, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var sales = await repository.GetSalesAsync(
                query.CafeId,
                requestContext.ManagementKey,
                query.FromUtc,
                query.ToUtc,
                query.CustomerId,
                query.MenuItemId,
                ct);
            return sales.Select(sale => new Response
            {
                OrderId = sale.Order.Id,
                CustomerId = sale.Order.CustomerId,
                Status = sale.Order.Status.ToString(),
                TotalAmount = sale.Order.TotalAmount,
                CreatedAtUtc = sale.Order.CreatedAtUtc,
                Items = sale.Order.Items.Select(item => new ItemResponse(
                    item.MenuItemId,
                    item.Name,
                    item.UnitPrice,
                    item.Quantity,
                    item.UnitPrice * item.Quantity)).ToList()
            }).ToList();
        }
    }
}
