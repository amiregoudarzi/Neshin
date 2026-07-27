using Neshin.Domain.Common;

namespace Neshin.Domain.Customers;

public sealed class CustomerProfile : AggregateRoot
{
    private CustomerProfile(Guid id, DateTime createdAtUtc) : base(id)
    {
        CreatedAtUtc = createdAtUtc;
    }

    private CustomerProfile() : base(Guid.Empty) { }

    public Guid? UserId { get; private set; }
    public string? DisplayName { get; private set; }
    public string? ContactPhoneNumber { get; private set; }
    public bool IsPhoneNumberVerified { get; private set; }
    public DateTime CreatedAtUtc { get; private init; }

    public static CustomerProfile CreateGuest(DateTime createdAtUtc) =>
        new(Guid.NewGuid(), createdAtUtc);

    public static CustomerProfile CreateRegistered(
        Guid userId,
        string displayName,
        string phoneNumber,
        DateTime createdAtUtc)
    {
        if (userId == Guid.Empty) throw new DomainException("User is required.");

        var profile = new CustomerProfile(Guid.NewGuid(), createdAtUtc)
        {
            UserId = userId,
            DisplayName = NormalizeOptional(displayName, 100),
            ContactPhoneNumber = NormalizeOptional(phoneNumber, 30),
            IsPhoneNumberVerified = true
        };

        if (profile.DisplayName is null) throw new DomainException("Customer name is required.");
        return profile;
    }

    public void UpdateRegisteredContact(string displayName, string phoneNumber)
    {
        DisplayName = NormalizeOptional(displayName, 100)
            ?? throw new DomainException("Customer name is required.");
        ContactPhoneNumber = NormalizeOptional(phoneNumber, 30);
        IsPhoneNumberVerified = true;
    }

    public void SetOptionalContact(string? displayName, string? phoneNumber)
    {
        DisplayName = NormalizeOptional(displayName, 100);
        ContactPhoneNumber = NormalizeOptional(phoneNumber, 30);
        IsPhoneNumberVerified = false;
    }

    private static string? NormalizeOptional(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new DomainException($"The value cannot exceed {maximumLength} characters.");
        }

        return normalized;
    }
}
