using Neshin.Domain.Common;

namespace Neshin.Domain.Clients;

public sealed class Client : AggregateRoot
{
    private Client(Guid id, string name, DateTime createdAtUtc) : base(id)
    {
        Name = name;
        CreatedAtUtc = createdAtUtc;
    }
    private Client() : base(Guid.Empty) { }

    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private init; }

    public static Client Create(string name, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Client name is required.");
        }

        return new Client(Guid.NewGuid(), name.Trim(), createdAtUtc);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
