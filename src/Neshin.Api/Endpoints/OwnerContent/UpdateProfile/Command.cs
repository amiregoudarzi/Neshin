using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerContent.UpdateProfile;

internal sealed record Command : ICommand
{
    public Guid BranchId { get; init; }
    public string? Description { get; init; }
    public string? Address { get; init; }
    public string? PublicPhoneNumber { get; init; }
    public string? LogoUrl { get; init; }
    public string? CoverImageUrl { get; init; }
    private sealed class Handler(
        IOwnerExperienceRepository repository,
        IRequestContext requestContext) : ICommandHandler<Command>
    {
        public Task ExecuteAsync(Command command, CancellationToken ct) =>
            repository.UpdateBranchProfileAsync(
                command.BranchId,
                requestContext.ManagementKey,
                command.Description,
                command.Address,
                command.PublicPhoneNumber,
                command.LogoUrl,
                command.CoverImageUrl,
                ct);
    }
}
