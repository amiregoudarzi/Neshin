using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerContent.CreateMenu;

internal sealed record Command : ICommand<Command.Response>
{
    public Guid BranchId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool Publish { get; init; }
    internal sealed record Response(Guid Id);

    private sealed class Handler(
        IOwnerExperienceRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Response> ExecuteAsync(Command command, CancellationToken ct) =>
            new(await repository.CreateMenuAsync(
                command.BranchId,
                requestContext.ManagementKey,
                command.Name,
                command.Publish,
                ct));
    }
}
