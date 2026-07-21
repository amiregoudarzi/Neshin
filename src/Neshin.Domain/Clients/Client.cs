using Neshin.Domain.Common;

namespace Neshin.Domain.Clients;

public sealed class Client : AggregateRoot
{
    private Client(Guid id, string name) : base(id) => Name = name;
    private Client() : base(Guid.Empty) { }

    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public static Client Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Client name is required.");
        }

        return new Client(Guid.NewGuid(), name.Trim());
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
