using Microsoft.EntityFrameworkCore;
using Neshin.Application.Abstractions.Persistence;
using Neshin.Application.Common;
using Neshin.Domain.Customers;
using Neshin.Infrastructure.Persistence;

namespace Neshin.Infrastructure.CustomerExperience;

public sealed class PublicExperienceRepository(
    NeshinWriteDbContext writeDbContext,
    NeshinReadDbContext readDbContext,
    TimeProvider timeProvider) : IPublicExperienceRepository
{
    private const decimal AmbiguityDistanceMeters = 30;
    private const decimal MaximumDiscoveryRadiusMeters = 250;
    private const decimal MaximumAutomaticAccuracyMeters = 150;
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromDays(90);
    private static readonly TimeSpan VisitLifetime = TimeSpan.FromHours(4);

    public async Task<(CustomerProfile Customer, CustomerSession Session, string AccessToken)> CreateSessionAsync(
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var customer = CustomerProfile.CreateGuest(now);
        var accessToken = TokenHashing.CreateToken();
        var session = CustomerSession.Create(
            customer.Id,
            TokenHashing.Hash(accessToken),
            now,
            now.Add(SessionLifetime));

        writeDbContext.CustomerProfiles.Add(customer);
        writeDbContext.CustomerSessions.Add(session);
        await writeDbContext.SaveChangesAsync(cancellationToken);

        return (customer, session, accessToken);
    }

    public async Task<DiscoveryResult> DiscoverAsync(
        decimal latitude,
        decimal longitude,
        decimal accuracyMeters,
        decimal radiusMeters,
        CancellationToken cancellationToken = default)
    {
        var request = new LocationRequest(latitude, longitude, accuracyMeters, radiusMeters);
        ValidateDiscovery(request);

        var effectiveRadius = Math.Min(request.RadiusMeters, MaximumDiscoveryRadiusMeters);
        var latitudeDelta = effectiveRadius / 111_320m;
        var latitudeRadians = (double)request.Latitude * Math.PI / 180;
        var longitudeScale = Math.Max(0.01, Math.Abs(Math.Cos(latitudeRadians)));
        var longitudeDelta = effectiveRadius / (111_320m * (decimal)longitudeScale);

        var branches = await (
                from branch in readDbContext.Branches
                join client in readDbContext.Clients on branch.ClientId equals client.Id
                where branch.IsActive && client.IsActive &&
                      branch.Latitude >= request.Latitude - latitudeDelta &&
                      branch.Latitude <= request.Latitude + latitudeDelta &&
                      branch.Longitude >= request.Longitude - longitudeDelta &&
                      branch.Longitude <= request.Longitude + longitudeDelta
                select new
                {
                    Branch = branch,
                    ClientId = client.Id
                })
            .ToListAsync(cancellationToken);

        var matches = branches
            .Select(item => new
            {
                item.Branch,
                item.ClientId,
                Distance = CalculateDistanceMeters(
                    request.Latitude,
                    request.Longitude,
                    item.Branch.Latitude,
                    item.Branch.Longitude)
            })
            .Where(item => item.Distance <= effectiveRadius)
            .OrderBy(item => item.Distance)
            .ThenBy(item => item.Branch.Id)
            .Take(5)
            .Select(item => new DiscoveryMatch(
                item.Branch.Id,
                item.ClientId,
                item.Branch.Name,
                Math.Round(item.Distance, 2),
                GetConfidence(item.Distance, request.AccuracyMeters),
                item.Branch.AcceptsAppOrders))
            .ToList();

        if (matches.Count == 0)
            return new DiscoveryResult("none", null, matches);

        var hasUsefulAccuracy = request.AccuracyMeters <= MaximumAutomaticAccuracyMeters;
        var hasClearWinner = hasUsefulAccuracy &&
            matches[0].Confidence != "low" &&
            (matches.Count == 1 ||
             matches[1].DistanceMeters - matches[0].DistanceMeters >= AmbiguityDistanceMeters);

        return new DiscoveryResult(
            hasClearWinner ? "single" : "multiple",
            hasClearWinner ? matches[0].BranchId : null,
            matches);
    }

    public async Task<StorefrontData?> GetStorefrontAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        var branch = await (
                from candidate in readDbContext.Branches
                join client in readDbContext.Clients on candidate.ClientId equals client.Id
                where candidate.Id == branchId && candidate.IsActive && client.IsActive
                select new
                {
                    Branch = candidate,
                    ClientId = client.Id
                })
            .SingleOrDefaultAsync(cancellationToken);

        if (branch is null) return null;

        var menus = await readDbContext.Menus
            .Where(menu => menu.BranchId == branchId && menu.IsPublished)
            .OrderBy(menu => menu.Name)
            .ToListAsync(cancellationToken);

        var menuIds = menus.Select(menu => menu.Id).ToList();
        var items = await readDbContext.MenuItems
            .Where(item => menuIds.Contains(item.MenuId) && item.IsAvailable)
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Name)
            .ToListAsync(cancellationToken);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var events = await readDbContext.VenueEvents
            .Where(venueEvent =>
                venueEvent.BranchId == branchId &&
                venueEvent.IsPublished &&
                venueEvent.EndsAtUtc >= now)
            .OrderBy(venueEvent => venueEvent.StartsAtUtc)
            .ToListAsync(cancellationToken);

        return new StorefrontData(
            branch.Branch,
            branch.ClientId,
            menus,
            items,
            events);
    }

    public async Task<VenueVisit> StartOrRefreshVisitAsync(
        string sessionToken,
        Guid branchId,
        decimal latitude,
        decimal longitude,
        decimal accuracyMeters,
        CancellationToken cancellationToken = default)
    {
        var request = new LocationRequest(
            latitude,
            longitude,
            accuracyMeters,
            MaximumDiscoveryRadiusMeters);
        ValidateDiscovery(request);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var session = await ResolveSessionAsync(sessionToken, now, cancellationToken);
        var branch = await (
                from candidate in writeDbContext.Branches
                join client in writeDbContext.Clients on candidate.ClientId equals client.Id
                where candidate.Id == branchId && candidate.IsActive && client.IsActive
                select candidate)
            .SingleOrDefaultAsync(cancellationToken);

        if (branch is null) throw new ResourceNotFoundException("The venue was not found.");

        var distanceMeters = CalculateDistanceMeters(
            request.Latitude,
            request.Longitude,
            branch.Latitude,
            branch.Longitude);
        var permittedDistance = Math.Max(150m, request.AccuracyMeters);
        if (distanceMeters > permittedDistance)
            throw new Domain.Common.DomainException("The device is not close enough to start a venue visit.");

        var staleVisits = await writeDbContext.VenueVisits
            .Where(candidate =>
                candidate.CustomerId == session.CustomerId &&
                candidate.EndedAtUtc == null &&
                candidate.StartedAtUtc < now.Subtract(VisitLifetime))
            .ToListAsync(cancellationToken);
        foreach (var staleVisit in staleVisits) staleVisit.End(now);

        var visit = await writeDbContext.VenueVisits
            .Where(candidate =>
                candidate.CustomerId == session.CustomerId &&
                candidate.BranchId == branchId &&
                candidate.EndedAtUtc == null &&
                candidate.StartedAtUtc >= now.Subtract(VisitLifetime))
            .OrderByDescending(candidate => candidate.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (visit is null)
        {
            visit = VenueVisit.Start(
                branchId,
                session.CustomerId,
                distanceMeters,
                request.AccuracyMeters,
                now);
            writeDbContext.VenueVisits.Add(visit);
        }
        else
        {
            visit.Refresh(now);
        }

        session.Touch(now);
        await writeDbContext.SaveChangesAsync(cancellationToken);

        return visit;
    }

    private async Task<CustomerSession> ResolveSessionAsync(
        string token,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new RequestUnauthorizedException("A customer session token is required.");

        var hash = TokenHashing.Hash(token);
        var session = await writeDbContext.CustomerSessions.SingleOrDefaultAsync(
            candidate => candidate.TokenHash == hash,
            cancellationToken);

        if (session is null || !session.IsValidAt(now))
            throw new RequestUnauthorizedException("The customer session is invalid or expired.");

        return session;
    }

    private static void ValidateDiscovery(LocationRequest request)
    {
        if (request.Latitude is < -90 or > 90)
            throw new Domain.Common.DomainException("Latitude is out of range.");
        if (request.Longitude is < -180 or > 180)
            throw new Domain.Common.DomainException("Longitude is out of range.");
        if (request.AccuracyMeters is < 0 or > 1000)
            throw new Domain.Common.DomainException("Location accuracy is out of range.");
        if (request.RadiusMeters is < 25 or > 1000)
            throw new Domain.Common.DomainException("Discovery radius must be between 25 and 1000 meters.");
    }

    private static decimal CalculateDistanceMeters(
        decimal latitude,
        decimal longitude,
        decimal branchLatitude,
        decimal branchLongitude)
    {
        const double earthRadiusMeters = 6_371_000;
        var latitudeRadians = DegreesToRadians((double)latitude);
        var branchLatitudeRadians = DegreesToRadians((double)branchLatitude);
        var latitudeDelta = DegreesToRadians((double)(branchLatitude - latitude));
        var longitudeDelta = DegreesToRadians((double)(branchLongitude - longitude));

        var haversine = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2) +
                        Math.Cos(latitudeRadians) * Math.Cos(branchLatitudeRadians) *
                        Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);
        var distance = earthRadiusMeters * 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));
        return (decimal)distance;
    }

    private static double DegreesToRadians(double value) => value * Math.PI / 180;

    private static string GetConfidence(decimal distance, decimal accuracy) =>
        accuracy <= 50 && distance <= 75 ? "high" : accuracy <= 150 && distance <= 200 ? "medium" : "low";

    private sealed record LocationRequest(
        decimal Latitude,
        decimal Longitude,
        decimal AccuracyMeters,
        decimal RadiusMeters);
}
