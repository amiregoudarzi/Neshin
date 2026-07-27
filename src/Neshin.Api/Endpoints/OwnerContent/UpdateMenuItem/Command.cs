using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerContent.UpdateMenuItem;

internal sealed record Command : ICommand
{
    public Guid BranchId { get; init; }
    public Guid MenuId { get; init; }
    public Guid ItemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? CategoryName { get; init; }
    public string? ImageUrl { get; init; }
    public decimal Price { get; init; }
    public bool IsAvailable { get; init; }
    public int DisplayOrder { get; init; }
    private sealed class Handler(
        IOwnerExperienceRepository repository,
        IRequestContext requestContext) : ICommandHandler<Command>
    {
        public Task ExecuteAsync(Command command, CancellationToken ct) =>
            repository.UpdateMenuItemAsync(
                command.BranchId,
                command.MenuId,
                command.ItemId,
                requestContext.ManagementKey,
                command.Name,
                command.Description,
                command.CategoryName,
                command.ImageUrl,
                command.Price,
                command.IsAvailable,
                command.DisplayOrder,
                ct);
    }
}
