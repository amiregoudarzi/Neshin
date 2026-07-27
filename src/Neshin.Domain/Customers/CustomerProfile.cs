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
