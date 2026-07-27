using Neshin.Domain.Common;

namespace Neshin.Domain.Identity;

public sealed class User : AggregateRoot
{
    private User(Guid id, string phoneNumber, DateTime createdAtUtc)
        : base(id)
    {
        PhoneNumber = phoneNumber;
        CreatedAtUtc = createdAtUtc;
    }

    private User() : base(Guid.Empty)
    {
    }

    public string PhoneNumber { get; private set; } = string.Empty;
    public bool IsPhoneNumberVerified { get; private set; }
    public DateTime CreatedAtUtc { get; private init; }
    public DateTime? PhoneNumberVerifiedAtUtc { get; private set; }

    public static User Create(string phoneNumber, DateTime createdAtUtc)
    {
        var normalizedPhoneNumber = NormalizePhoneNumber(phoneNumber);
        return new User(Guid.NewGuid(), normalizedPhoneNumber, createdAtUtc);
    }

    public void VerifyPhoneNumber(DateTime verifiedAtUtc)
    {
        IsPhoneNumberVerified = true;
        PhoneNumberVerifiedAtUtc = verifiedAtUtc;
    }

    public static string NormalizePhoneNumber(string phoneNumber)
    {
        var value = phoneNumber.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);

        if (value.StartsWith("+98", StringComparison.Ordinal))
        {
            value = $"0{value[3..]}";
        }
        else if (value.StartsWith("0098", StringComparison.Ordinal))
        {
            value = $"0{value[4..]}";
        }

        if (value.Length != 11 || !value.StartsWith("09", StringComparison.Ordinal) || value.Any(character => !char.IsDigit(character)))
        {
            throw new DomainException("A valid Iranian mobile phone number is required.");
        }

        return value;
    }
}
