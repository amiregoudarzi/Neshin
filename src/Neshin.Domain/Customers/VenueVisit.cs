using Neshin.Domain.Common;

namespace Neshin.Domain.Customers;

public sealed class VenueVisit : Entity
{
    private VenueVisit(
        Guid id,
        Guid branchId,
        Guid customerId,
        decimal distanceMeters,
        decimal accuracyMeters,
        DateTime startedAtUtc) : base(id)
    {
        BranchId = branchId;
        CustomerId = customerId;
        DistanceMeters = distanceMeters;
        AccuracyMeters = accuracyMeters;
        CreatedAtUtc = startedAtUtc;
        StartedAtUtc = startedAtUtc;
        LastSeenAtUtc = startedAtUtc;
    }

    private VenueVisit() : base(Guid.Empty) { }

    public Guid BranchId { get; private init; }
    public Guid CustomerId { get; private init; }
    public decimal DistanceMeters { get; private init; }
    public decimal AccuracyMeters { get; private init; }
    public DateTime StartedAtUtc { get; private init; }
    public DateTime CreatedAtUtc { get; private init; }
    public DateTime LastSeenAtUtc { get; private set; }
    public DateTime? EndedAtUtc { get; private set; }

    public static VenueVisit Start(
        Guid branchId,
        Guid customerId,
        decimal distanceMeters,
        decimal accuracyMeters,
        DateTime startedAtUtc)
    {
        if (branchId == Guid.Empty || customerId == Guid.Empty)
            throw new DomainException("Branch and customer are required.");

        return new VenueVisit(
            Guid.NewGuid(),
            branchId,
            customerId,
            Math.Max(0, distanceMeters),
            Math.Max(0, accuracyMeters),
            startedAtUtc);
    }

    public void Refresh(DateTime now) => LastSeenAtUtc = now;
    public void End(DateTime now) => EndedAtUtc ??= now;
}
