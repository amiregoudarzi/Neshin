using Microsoft.EntityFrameworkCore;
using Neshin.Application.Abstractions.Persistence;
using Neshin.Domain.Identity;

namespace Neshin.Infrastructure.Persistence.Repositories;

public sealed class UserWriteRepository(NeshinWriteDbContext dbContext) : IUserWriteRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.PhoneNumber == phoneNumber, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await dbContext.Users.AddAsync(user, cancellationToken);
}
