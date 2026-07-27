using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerCustomers.Archive;

internal sealed record Command : ICommand
{
    public Guid BranchId { get; init; }
    public Guid CustomerId { get; init; }
    private sealed class Handler(
        IOwnerExperienceRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Command>
    {
        public Task ExecuteAsync(Command command, CancellationToken ct)
        {
            requestContext.SetNoStore();
            return repository.ArchiveCustomerAsync(
                command.BranchId, command.CustomerId, requestContext.ManagementKey, ct);
        }
    }
}
