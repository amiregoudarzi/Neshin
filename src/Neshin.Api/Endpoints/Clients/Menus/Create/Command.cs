using FastEndpoints;

namespace Neshin.Api.Endpoints.Clients.Menus.Create;

internal sealed record Command : ICommand<Command.Response>
{
    public Guid CafeId { get; init; }
    public string Title { get; init; } = string.Empty;
    public bool Publish { get; init; }
    public IReadOnlyList<Item> Items { get; init; } = [];

    internal sealed record Item
    {
        public string Title { get; init; } = string.Empty;
        public string? Caption { get; init; }
        public string? Category { get; init; }
        public string? PhotoUrl { get; init; }
        public decimal Price { get; init; }
        public bool IsAvailable { get; init; } = true;
        public int DisplayOrder { get; init; }
    }

    internal sealed record Response(Guid MenuId, IReadOnlyList<Guid> ItemIds);

    private sealed class Handler(
        IClientExperienceRepository repository,
        IRequestContext requestContext) : ICommandHandler<Command, Response>
    {
        public async Task<Response> ExecuteAsync(Command command, CancellationToken ct)
        {
            var result = await repository.CreateMenuAsync(
                command.CafeId,
                requestContext.ManagementKey,
                command.Title,
                command.Publish,
                command.Items.Select(ToInput).ToList(),
                ct);
            return new Response(result.MenuId, result.ItemIds);
        }

        private static MenuItemInput ToInput(Item item) => new(
            item.Title,
            item.Caption,
            item.Category,
            item.PhotoUrl,
            item.Price,
            item.IsAvailable,
            item.DisplayOrder);
    }
}
