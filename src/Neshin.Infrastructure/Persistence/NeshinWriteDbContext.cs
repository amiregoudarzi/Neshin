using Microsoft.EntityFrameworkCore;
using Neshin.Domain.Catalog;
using Neshin.Domain.Clients;
using Neshin.Domain.Identity;
using Neshin.Domain.Ordering;

namespace Neshin.Infrastructure.Persistence;

public sealed class NeshinWriteDbContext(DbContextOptions<NeshinWriteDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NeshinWriteDbContext).Assembly);
}
