using Neshin.Domain.Common;

namespace Neshin.Domain.Customers;

public sealed class BranchCustomer : Entity
{
    private BranchCustomer(
        Guid id,
        Guid branchId,
        Guid customerId,
        string source,
        string? contactPhoneNumber,
        DateTime addedAtUtc) : base(id)
    {
        BranchId = branchId;
        CustomerId = customerId;
        Source = source;
        ContactPhoneNumber = contactPhoneNumber;
        CreatedAtUtc = addedAtUtc;
        AddedAtUtc = addedAtUtc;
    }

    private BranchCustomer() : base(Guid.Empty) { }

    public Guid BranchId { get; private init; }
    public Guid CustomerId { get; private init; }
    public string Source { get; private init; } = string.Empty;
    public string? Notes { get; private set; }
    public string? ContactPhoneNumber { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime AddedAtUtc { get; private init; }
    public DateTime CreatedAtUtc { get; private init; }
    public DateTime? ArchivedAtUtc { get; private set; }

    public static BranchCustomer Add(
        Guid branchId,
        Guid customerId,
        string source,
        string? contactPhoneNumber,
        DateTime addedAtUtc)
    {
        if (branchId == Guid.Empty || customerId == Guid.Empty)
            throw new DomainException("Branch and customer are required.");
        if (string.IsNullOrWhiteSpace(source)) throw new DomainException("CRM source is required.");

        return new BranchCustomer(
            Guid.NewGuid(),
            branchId,
            customerId,
            source.Trim(),
            contactPhoneNumber,
            addedAtUtc);
    }

    public void Restore()
    {
        IsArchived = false;
        ArchivedAtUtc = null;
    }

    public void SetConsentedPhoneNumber(string? phoneNumber) =>
        ContactPhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();

    public void Archive(DateTime now)
    {
        IsArchived = true;
        ArchivedAtUtc ??= now;
    }

    public void SetNotes(string? notes)
    {
        if (notes?.Length > 1000) throw new DomainException("CRM notes cannot exceed 1000 characters.");
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
