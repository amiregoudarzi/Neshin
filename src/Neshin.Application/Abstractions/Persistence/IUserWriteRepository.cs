using Neshin.Domain.Identity;

namespace Neshin.Application.Abstractions.Persistence;

public interface IUserWriteRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    public Task AddAsync(User user, CancellationToken cancellationToken = default);
}
