using FastEndpoints;

namespace Neshin.Api.Endpoints.Clients.MenuItems.Update;

internal sealed record Command : ICommand
{
    public Guid CafeId { get; init; }
    public Guid MenuId { get; init; }
    public Guid ItemId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Caption { get; init; }
    public string? Category { get; init; }
    public string? PhotoUrl { get; init; }
    public decimal Price { get; init; }
    public bool IsAvailable { get; init; }
    public int DisplayOrder { get; init; }

    private sealed class Handler(
        IClientExperienceRepository repository,
        IRequestContext requestContext) : ICommandHandler<Command>
    {
        public Task ExecuteAsync(Command command, CancellationToken ct) =>
            repository.UpdateMenuItemAsync(
                command.CafeId,
                command.MenuId,
                command.ItemId,
                requestContext.ManagementKey,
                new MenuItemInput(
                    command.Title,
                    command.Caption,
                    command.Category,
                    command.PhotoUrl,
                    command.Price,
                    command.IsAvailable,
                    command.DisplayOrder),
                ct);
    }
}
