using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerVisits.List;

internal sealed record Query : ICommand<IReadOnlyList<Query.Response>>
{
    public Guid BranchId { get; init; }
    private sealed class Handler(
        IOwnerExperienceRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Query, IReadOnlyList<Response>>
    {
        public async Task<IReadOnlyList<Response>> ExecuteAsync(Query query, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var visits = await repository.GetActiveVisitsAsync(
                query.BranchId, requestContext.ManagementKey, ct);
            return visits.Select(item => new Response
            {
                VisitId = item.Visit.Id,
                StartedAtUtc = item.Visit.StartedAtUtc,
                LastSeenAtUtc = item.Visit.LastSeenAtUtc,
                HasOpenOrder = item.HasOpenOrder
            }).ToList();
        }
    }

    internal sealed record Response
    {
        public Guid VisitId { get; init; }
        public DateTime StartedAtUtc { get; init; }
        public DateTime LastSeenAtUtc { get; init; }
        public bool HasOpenOrder { get; init; }
    }
}
