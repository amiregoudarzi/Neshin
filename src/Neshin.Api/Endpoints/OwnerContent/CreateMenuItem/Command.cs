using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerContent.CreateMenuItem;

internal sealed record Command : ICommand<Command.Response>
{
    public Guid BranchId { get; init; }
    public Guid MenuId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? CategoryName { get; init; }
    public string? ImageUrl { get; init; }
    public decimal Price { get; init; }
    public bool IsAvailable { get; init; }
    public int DisplayOrder { get; init; }
    internal sealed record Response(Guid Id);

    private sealed class Handler(
        IOwnerExperienceRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Response> ExecuteAsync(Command command, CancellationToken ct) =>
            new(await repository.CreateMenuItemAsync(
                command.BranchId,
                command.MenuId,
                requestContext.ManagementKey,
                command.Name,
                command.Description,
                command.CategoryName,
                command.ImageUrl,
                command.Price,
                command.IsAvailable,
                command.DisplayOrder,
                ct));
    }
}
