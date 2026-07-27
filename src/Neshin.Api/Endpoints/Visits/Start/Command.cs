using FastEndpoints;

namespace Neshin.Api.Endpoints.Visits.Start;

internal sealed record Command : ICommand<Command.Response>
{
    public Guid BranchId { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public decimal AccuracyMeters { get; init; }
    private sealed class Handler(
        IPublicExperienceRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Response> ExecuteAsync(Command command, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var visit = await repository.StartOrRefreshVisitAsync(
                requestContext.CustomerSessionToken,
                command.BranchId,
                command.Latitude,
                command.Longitude,
                command.AccuracyMeters,
                ct);
            return new Response
            {
                VisitId = visit.Id,
                BranchId = visit.BranchId,
                LastSeenAtUtc = visit.LastSeenAtUtc
            };
        }
    }

    internal sealed record Response
    {
        public Guid VisitId { get; init; }
        public Guid BranchId { get; init; }
        public DateTime LastSeenAtUtc { get; init; }
    }
}
