using Neshin.Domain.Common;

namespace Neshin.Domain.Clients;

public sealed class Branch : AggregateRoot
{
    private Branch(
        Guid id,
        Guid clientId,
        string name,
        decimal latitude,
        decimal longitude,
        DateTime createdAtUtc)
        : base(id)
    {
        ClientId = clientId;
        Name = name;
        Latitude = latitude;
        Longitude = longitude;
        CreatedAtUtc = createdAtUtc;
    }

    private Branch() : base(Guid.Empty) { }

    public Guid ClientId { get; private init; }
    public string Name { get; private set; } = string.Empty;
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public bool IsActive { get; private set; }
    public bool AcceptsAppOrders { get; private set; }
    public bool AllowsPayAtVenue { get; private set; }
    public string? Description { get; private set; }
    public string? Address { get; private set; }
    public string? PublicPhoneNumber { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public DateTime CreatedAtUtc { get; private init; }

    public static Branch Create(
        Guid clientId,
        string name,
        decimal latitude,
        decimal longitude,
        DateTime createdAtUtc)
    {
        if (clientId == Guid.Empty) throw new DomainException("Client is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Branch name is required.");
        ValidateCoordinates(latitude, longitude);

        return new Branch(Guid.NewGuid(), clientId, name.Trim(), latitude, longitude, createdAtUtc);
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

    public void UpdatePublicProfile(
        string? description,
        string? address,
        string? publicPhoneNumber,
        string? logoUrl,
        string? coverImageUrl)
    {
        Description = NormalizeOptional(description, 2000);
        Address = NormalizeOptional(address, 500);
        PublicPhoneNumber = NormalizeOptional(publicPhoneNumber, 30);
        LogoUrl = NormalizeOptional(logoUrl, 2000);
        CoverImageUrl = NormalizeOptional(coverImageUrl, 2000);
    }

    private static void ValidateCoordinates(decimal latitude, decimal longitude)
    {
        if (latitude is < -90 or > 90) throw new DomainException("Latitude is out of range.");
        if (longitude is < -180 or > 180) throw new DomainException("Longitude is out of range.");
    }

    private static string? NormalizeOptional(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
            throw new DomainException($"The value cannot exceed {maximumLength} characters.");
        return normalized;
    }
}
