using Microsoft.EntityFrameworkCore;
using Neshin.Domain.Catalog;
using Neshin.Domain.Clients;
using Neshin.Domain.Customers;
using Neshin.Domain.Identity;
using Neshin.Domain.Ordering;

namespace Neshin.Infrastructure.Persistence;

public sealed class NeshinReadDbContext(DbContextOptions<NeshinReadDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Branch> Branches { get; set; } = null!;
    public DbSet<Menu> Menus { get; set; } = null!;
    public DbSet<MenuItem> MenuItems { get; set; } = null!;
    public DbSet<VenueEvent> VenueEvents { get; set; } = null!;
    public DbSet<CustomerProfile> CustomerProfiles { get; set; } = null!;
    public DbSet<CustomerSession> CustomerSessions { get; set; } = null!;
    public DbSet<VenueVisit> VenueVisits { get; set; } = null!;
    public DbSet<BranchCustomer> BranchCustomers { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<InvoiceItem> InvoiceItems { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        NeshinWriteDbContext.ApplyConfigurations(builder);
        base.OnModelCreating(builder);
    }
}
