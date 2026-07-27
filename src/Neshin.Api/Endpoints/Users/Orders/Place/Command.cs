using FastEndpoints;

namespace Neshin.Api.Endpoints.Users.Orders.Place;

internal sealed record Command : ICommand<Command.Response>
{
    public Guid CafeId { get; init; }
    public IReadOnlyList<Item> Items { get; init; } = [];

    internal sealed record Item(Guid MenuItemId, int Quantity);

    internal sealed record Response
    {
        public Guid OrderId { get; init; }
        public string Status { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public int Version { get; init; }
        public DateTime CreatedAtUtc { get; init; }
    }

    private sealed class Handler(
        IUserExperienceRepository repository,
        IRequestContext requestContext) : ICommandHandler<Command, Response>
    {
        public async Task<Response> ExecuteAsync(Command command, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var order = await repository.PlaceOrderAsync(
                requestContext.CustomerSessionToken,
                command.CafeId,
                requestContext.IdempotencyKey,
                command.Items.Select(item => (item.MenuItemId, item.Quantity)).ToList(),
                ct);
            return new Response
            {
                OrderId = order.Id,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                Version = order.Version,
                CreatedAtUtc = order.CreatedAtUtc
            };
        }
    }
}
