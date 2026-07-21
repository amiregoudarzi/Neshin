using Neshin.Application.Abstractions.Persistence;

namespace Neshin.Infrastructure.Persistence;

public sealed class UnitOfWork(NeshinWriteDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
