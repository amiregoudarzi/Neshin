using Neshin.Domain.Common;

namespace Neshin.Domain.Customers;

public sealed class CustomerSession : Entity
{
    private CustomerSession(
        Guid id,
        Guid customerId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc) : base(id)
    {
        CustomerId = customerId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        LastSeenAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    private CustomerSession() : base(Guid.Empty) { }

    public Guid CustomerId { get; private init; }
    public string TokenHash { get; private init; } = string.Empty;
    public DateTime CreatedAtUtc { get; private init; }
    public DateTime LastSeenAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private init; }
    public DateTime? RevokedAtUtc { get; private set; }

    public bool IsValidAt(DateTime now) => RevokedAtUtc is null && ExpiresAtUtc > now;

    public static CustomerSession Create(
        Guid customerId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        if (customerId == Guid.Empty) throw new DomainException("Customer is required.");
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new DomainException("Session token hash is required.");
        if (expiresAtUtc <= createdAtUtc) throw new DomainException("Session expiry must be after creation.");

        return new CustomerSession(Guid.NewGuid(), customerId, tokenHash, createdAtUtc, expiresAtUtc);
    }

    public void Touch(DateTime now)
    {
        if (!IsValidAt(now)) throw new DomainException("The customer session has expired.");
        LastSeenAtUtc = now;
    }

    public void Revoke(DateTime now) => RevokedAtUtc ??= now;
}
