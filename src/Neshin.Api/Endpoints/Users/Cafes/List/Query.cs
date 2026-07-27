using FastEndpoints;

namespace Neshin.Api.Endpoints.Users.Cafes.List;

internal sealed record Query : ICommand<IReadOnlyList<Query.Response>>
{
    public bool NearbyOnly { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public decimal RadiusMeters { get; init; } = 1000;

    internal sealed record Response
    {
        public Guid CafeId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? Address { get; init; }
        public string? PhoneNumber { get; init; }
        public decimal Latitude { get; init; }
        public decimal Longitude { get; init; }
        public decimal? DistanceMeters { get; init; }
        public IReadOnlyList<string> PhotoUrls { get; init; } = [];
        public bool AcceptsOrders { get; init; }
    }

    private sealed class Handler(IUserExperienceRepository repository)
        : ICommandHandler<Query, IReadOnlyList<Response>>
    {
        public async Task<IReadOnlyList<Response>> ExecuteAsync(Query query, CancellationToken ct)
        {
            var cafes = await repository.GetCafesAsync(
                query.NearbyOnly,
                query.Latitude,
                query.Longitude,
                query.RadiusMeters,
                ct);
            return cafes.Select(result => new Response
            {
                CafeId = result.Cafe.Id,
                Name = result.Cafe.Name,
                Description = result.Cafe.Description,
                Address = result.Cafe.Address,
                PhoneNumber = result.Cafe.PublicPhoneNumber,
                Latitude = result.Cafe.Latitude,
                Longitude = result.Cafe.Longitude,
                DistanceMeters = result.DistanceMeters,
                PhotoUrls = result.Cafe.PhotoUrls,
                AcceptsOrders = result.Cafe.AcceptsAppOrders
            }).ToList();
        }
    }
}
