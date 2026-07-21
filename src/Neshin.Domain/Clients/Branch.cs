using Neshin.Domain.Common;

namespace Neshin.Domain.Clients;

public sealed class Branch : AggregateRoot
{
    private Branch(Guid id, Guid clientId, string name, decimal latitude, decimal longitude)
        : base(id)
    {
        ClientId = clientId;
        Name = name;
        Latitude = latitude;
        Longitude = longitude;
    }

    private Branch() : base(Guid.Empty) { }

    public Guid ClientId { get; private init; }
    public string Name { get; private set; } = string.Empty;
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public bool IsActive { get; private set; }
    public bool AcceptsAppOrders { get; private set; }
    public bool AllowsPayAtVenue { get; private set; }

    public static Branch Create(Guid clientId, string name, decimal latitude, decimal longitude)
    {
        if (clientId == Guid.Empty) throw new DomainException("Client is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Branch name is required.");
        ValidateCoordinates(latitude, longitude);

        return new Branch(Guid.NewGuid(), clientId, name.Trim(), latitude, longitude);
    }

    public void Activate() => IsActive = true;

    public void Deactivate()
    {
        IsActive = false;
        AcceptsAppOrders = false;
    }

    public void SetAppOrdering(bool enabled)
    {
        if (enabled && !IsActive)
        {
            throw new DomainException("An inactive branch cannot accept app orders.");
        }

        AcceptsAppOrders = enabled;
    }

    public void SetPayAtVenue(bool enabled) => AllowsPayAtVenue = enabled;

    private static void ValidateCoordinates(decimal latitude, decimal longitude)
    {
        if (latitude is < -90 or > 90) throw new DomainException("Latitude is out of range.");
        if (longitude is < -180 or > 180) throw new DomainException("Longitude is out of range.");
    }
}
