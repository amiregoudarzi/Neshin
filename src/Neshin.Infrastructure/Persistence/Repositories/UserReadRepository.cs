using Microsoft.EntityFrameworkCore;
using Neshin.Application.Abstractions.Persistence;

namespace Neshin.Infrastructure.Persistence.Repositories;

public sealed class UserReadRepository(NeshinReadDbContext dbContext) : IUserReadRepository
{
    public Task<UserReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Users
            .Where(user => user.Id == id)
            .Select(user => new UserReadModel(user.Id, user.PhoneNumber, user.IsPhoneNumberVerified, user.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
}
