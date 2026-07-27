using FastEndpoints;

namespace Neshin.Api.Endpoints.Discovery.Resolve;

internal sealed record Query : ICommand<Query.Response>
{
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public decimal AccuracyMeters { get; init; }
    public decimal RadiusMeters { get; init; } = 150;

    private sealed class Handler(
        IPublicExperienceRepository repository,
        IRequestContext requestContext)
        : ICommandHandler<Query, Response>
    {
        public async Task<Response> ExecuteAsync(Query query, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var result = await repository.DiscoverAsync(
                query.Latitude,
                query.Longitude,
                query.AccuracyMeters,
                query.RadiusMeters,
                ct);
            return new Response
            {
                Resolution = result.Resolution,
                SuggestedBranchId = result.SuggestedBranchId,
                Matches = result.Matches.Select(match => new MatchResponse
                {
                    BranchId = match.BranchId,
                    ClientId = match.ClientId,
                    Name = match.Name,
                    DistanceMeters = match.DistanceMeters,
                    Confidence = match.Confidence,
                    AcceptsOrders = match.AcceptsOrders
                }).ToList()
            };
        }
    }

    internal sealed record Response
    {
        public string Resolution { get; init; } = string.Empty;
        public Guid? SuggestedBranchId { get; init; }
        public IReadOnlyList<MatchResponse> Matches { get; init; } = [];
    }

    internal sealed record MatchResponse
    {
        public Guid BranchId { get; init; }
        public Guid ClientId { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal DistanceMeters { get; init; }
        public string Confidence { get; init; } = string.Empty;
        public bool AcceptsOrders { get; init; }
    }
}
