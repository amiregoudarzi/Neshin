using Neshin.Domain.Common;

namespace Neshin.Domain.Clients;

public sealed class VenueEvent : Entity
{
    private VenueEvent(
        Guid id,
        Guid branchId,
        string title,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        DateTime createdAtUtc) : base(id)
    {
        BranchId = branchId;
        Title = title;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    private VenueEvent() : base(Guid.Empty) { }

    public Guid BranchId { get; private init; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateTime StartsAtUtc { get; private set; }
    public DateTime EndsAtUtc { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime CreatedAtUtc { get; private init; }

    public static VenueEvent Create(
        Guid branchId,
        string title,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        DateTime createdAtUtc)
    {
        if (branchId == Guid.Empty) throw new DomainException("Branch is required.");
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Event title is required.");
        if (endsAtUtc <= startsAtUtc) throw new DomainException("Event end must be after its start.");

        return new VenueEvent(Guid.NewGuid(), branchId, title.Trim(), startsAtUtc, endsAtUtc, createdAtUtc);
    }

    public void Publish() => IsPublished = true;
    public void Unpublish() => IsPublished = false;

    public void Update(
        string title,
        string? description,
        string? imageUrl,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        bool isPublished)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Event title is required.");
        if (endsAtUtc <= startsAtUtc) throw new DomainException("Event end must be after its start.");
        if (description?.Length > 2000) throw new DomainException("Event description is too long.");
        if (imageUrl?.Length > 2000) throw new DomainException("Event image URL is too long.");

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        IsPublished = isPublished;
    }
}
