using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerContent.UpdateEvent;

internal sealed record Command : ICommand
{
    public Guid BranchId { get; init; }
    public Guid EventId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public DateTime StartsAtUtc { get; init; }
    public DateTime EndsAtUtc { get; init; }
    public bool IsPublished { get; init; }
    private sealed class Handler(
        IOwnerExperienceRepository repository,
        IRequestContext requestContext) : ICommandHandler<Command>
    {
        public Task ExecuteAsync(Command command, CancellationToken ct) =>
            repository.UpdateVenueEventAsync(
                command.BranchId,
                command.EventId,
                requestContext.ManagementKey,
                command.Title,
                command.Description,
                command.ImageUrl,
                command.StartsAtUtc,
                command.EndsAtUtc,
                command.IsPublished,
                ct);
    }
}
