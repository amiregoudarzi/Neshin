using Neshin.Domain.Catalog;
using Neshin.Domain.Clients;
using Neshin.Domain.Customers;

namespace Neshin.Application.Abstractions.Persistence;

public interface IPublicExperienceRepository
{
    public Task<(CustomerProfile Customer, CustomerSession Session, string AccessToken)>
        CreateSessionAsync(CancellationToken cancellationToken = default);

    public Task<DiscoveryResult> DiscoverAsync(
        decimal latitude,
        decimal longitude,
        decimal accuracyMeters,
        decimal radiusMeters,
        CancellationToken cancellationToken = default);

    public Task<StorefrontData?> GetStorefrontAsync(
        Guid branchId,
        CancellationToken cancellationToken = default);

    public Task<VenueVisit> StartOrRefreshVisitAsync(
        string sessionToken,
        Guid branchId,
        decimal latitude,
        decimal longitude,
        decimal accuracyMeters,
        CancellationToken cancellationToken = default);
}

public sealed record DiscoveryResult(
    string Resolution,
    Guid? SuggestedBranchId,
    IReadOnlyList<DiscoveryMatch> Matches);

public sealed record DiscoveryMatch(
    Guid BranchId,
    Guid ClientId,
    string Name,
    decimal DistanceMeters,
    string Confidence,
    bool AcceptsOrders);

public sealed record StorefrontData(
    Branch Branch,
    Guid ClientId,
    IReadOnlyList<Menu> Menus,
    IReadOnlyList<MenuItem> MenuItems,
    IReadOnlyList<VenueEvent> Events);
