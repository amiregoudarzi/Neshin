using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerContent.CreateEvent;

internal sealed record Command : ICommand<Command.Response>
{
    public Guid BranchId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public DateTime StartsAtUtc { get; init; }
    public DateTime EndsAtUtc { get; init; }
    public bool IsPublished { get; init; }
    internal sealed record Response(Guid Id);

    private sealed class Handler(
        IOwnerExperienceRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Response> ExecuteAsync(Command command, CancellationToken ct) =>
            new(await repository.CreateVenueEventAsync(
                command.BranchId,
                requestContext.ManagementKey,
                command.Title,
                command.Description,
                command.ImageUrl,
                command.StartsAtUtc,
                command.EndsAtUtc,
                command.IsPublished,
                ct));
    }
}
