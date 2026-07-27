using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Neshin.Application.Abstractions.Persistence;
using Neshin.Application.Common;
using Neshin.Domain.Customers;
using Neshin.Domain.Catalog;
using Neshin.Domain.Clients;
using Neshin.Domain.Ordering;
using Neshin.Infrastructure.Persistence;
using Npgsql;

namespace Neshin.Infrastructure.CustomerExperience;

public sealed class OwnerExperienceRepository(
    NeshinWriteDbContext writeDbContext,
    NeshinReadDbContext readDbContext,
    IConfiguration configuration,
    TimeProvider timeProvider) : IOwnerExperienceRepository
{
    private static readonly OrderStatus[] DefaultQueueStatuses =
    [
        OrderStatus.Submitted,
        OrderStatus.Accepted,
        OrderStatus.Preparing,
        OrderStatus.Ready
    ];

    public async Task<IReadOnlyList<Order>> GetOrdersAsync(
        Guid branchId,
        string managementKey,
        string? status,
        CancellationToken cancellationToken = default)
    {
        Authorize(branchId, managementKey);

        OrderStatus[] statuses;
        if (string.IsNullOrWhiteSpace(status))
        {
            statuses = DefaultQueueStatuses;
        }
        else if (Enum.TryParse<OrderStatus>(status, true, out var parsed) && Enum.IsDefined(parsed))
        {
            statuses = [parsed];
        }
        else
        {
            throw new Domain.Common.DomainException("The order status filter is invalid.");
        }

        var orders = await readDbContext.Orders
            .Include(order => order.Items)
            .Where(order => order.BranchId == branchId && statuses.Contains(order.Status))
            .OrderByDescending(order => order.CreatedAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);

        return orders;
    }

    public async Task<Order> ChangeOrderStatusAsync(
        Guid branchId,
        Guid orderId,
        string managementKey,
        string action,
        string? reason,
        int expectedVersion,
        CancellationToken cancellationToken = default)
    {
        Authorize(branchId, managementKey);
        var order = await writeDbContext.Orders
            .Include(candidate => candidate.Items)
            .SingleOrDefaultAsync(
                candidate => candidate.Id == orderId && candidate.BranchId == branchId,
                cancellationToken)
            ?? throw new ResourceNotFoundException("The order was not found.");

        if (order.Version != expectedVersion)
            throw new RequestConflictException("The order changed. Refresh it before applying this action.");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        action = action.Trim().ToLowerInvariant();
        if (action is not ("accept" or "reject" or "start-preparing" or "ready" or "complete"))
            throw new Domain.Common.DomainException("The order action is invalid.");
        if (action == "reject" && string.IsNullOrWhiteSpace(reason))
            throw new Domain.Common.DomainException("A rejection reason is required.");
        if (reason?.Length > 500)
            throw new Domain.Common.DomainException("The rejection reason cannot exceed 500 characters.");

        try
        {
            switch (action)
            {
                case "accept": order.Accept(now); break;
                case "reject": order.Reject(reason!, now); break;
                case "start-preparing": order.StartPreparing(); break;
                case "ready": order.MarkReady(now); break;
                case "complete": order.Complete(now); break;
            }
        }
        catch (Domain.Common.DomainException exception)
        {
            throw new RequestConflictException(exception.Message);
        }

        try
        {
            await writeDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new RequestConflictException("The order was updated by another staff member.")
            {
                Source = exception.Source
            };
        }

        return order;
    }

    public async Task<IReadOnlyList<(VenueVisit Visit, bool HasOpenOrder)>> GetActiveVisitsAsync(
        Guid branchId,
        string managementKey,
        CancellationToken cancellationToken = default)
    {
        Authorize(branchId, managementKey);
        var activeSince = timeProvider.GetUtcNow().UtcDateTime.Subtract(TimeSpan.FromMinutes(15));
        var visits = await readDbContext.VenueVisits
            .Where(visit =>
                visit.BranchId == branchId &&
                visit.EndedAtUtc == null &&
                visit.LastSeenAtUtc >= activeSince)
            .OrderByDescending(visit => visit.LastSeenAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);

        var customerIds = visits.Select(visit => visit.CustomerId).ToList();
        var customersWithOpenOrders = await readDbContext.Orders
            .Where(order =>
                order.BranchId == branchId &&
                customerIds.Contains(order.CustomerId) &&
                DefaultQueueStatuses.Contains(order.Status))
            .Select(order => order.CustomerId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return visits
            .Select(visit => (visit, customersWithOpenOrders.Contains(visit.CustomerId)))
            .ToList();
    }

    public async Task<IReadOnlyList<(BranchCustomer Relation, CustomerProfile Profile)>> GetCustomersAsync(
        Guid branchId,
        string managementKey,
        bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        Authorize(branchId, managementKey);
        var records = await (
                from relation in readDbContext.BranchCustomers
                join profile in readDbContext.CustomerProfiles on relation.CustomerId equals profile.Id
                where relation.BranchId == branchId && (includeArchived || !relation.IsArchived)
                orderby relation.AddedAtUtc descending
                select new
                {
                    Relation = relation,
                    Profile = profile
                })
            .Take(200)
            .ToListAsync(cancellationToken);

        return records.Select(record => (record.Relation, record.Profile)).ToList();
    }

    public async Task<(BranchCustomer Relation, CustomerProfile Profile)> AddCustomerAsync(
        Guid branchId,
        Guid customerId,
        string managementKey,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        Authorize(branchId, managementKey);
        var profile = await writeDbContext.CustomerProfiles.SingleOrDefaultAsync(
            customer => customer.Id == customerId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("The customer was not found.");

        var hasBranchRelationship = await writeDbContext.Orders.AnyAsync(
                order => order.BranchId == branchId && order.CustomerId == customerId,
                cancellationToken) ||
            await writeDbContext.VenueVisits.AnyAsync(
                visit => visit.BranchId == branchId && visit.CustomerId == customerId,
                cancellationToken);
        if (!hasBranchRelationship)
            throw new ResourceNotFoundException("The customer was not found for this branch.");

        var relation = await writeDbContext.BranchCustomers.SingleOrDefaultAsync(
            customer => customer.BranchId == branchId && customer.CustomerId == customerId,
            cancellationToken);

        if (relation is null)
        {
            var consentedPhoneNumber = await writeDbContext.Orders
                .Where(
                order => order.BranchId == branchId &&
                         order.CustomerId == customerId &&
                         order.AllowsPhoneContact)
                .OrderByDescending(order => order.CreatedAtUtc)
                .Select(order => order.ContactPhoneNumber)
                .FirstOrDefaultAsync(cancellationToken);
            relation = BranchCustomer.Add(
                branchId,
                customerId,
                "OwnerAdded",
                consentedPhoneNumber,
                timeProvider.GetUtcNow().UtcDateTime);
            writeDbContext.BranchCustomers.Add(relation);
        }
        else
        {
            relation.Restore();
            var consentedPhoneNumber = await writeDbContext.Orders
                .Where(
                order => order.BranchId == branchId &&
                         order.CustomerId == customerId &&
                         order.AllowsPhoneContact)
                .OrderByDescending(order => order.CreatedAtUtc)
                .Select(order => order.ContactPhoneNumber)
                .FirstOrDefaultAsync(cancellationToken);
            relation.SetConsentedPhoneNumber(consentedPhoneNumber);
        }

        relation.SetNotes(notes);
        try
        {
            await writeDbContext.SaveChangesAsync(cancellationToken);
            return (relation, profile);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            writeDbContext.Entry(relation).State = EntityState.Detached;
            var winner = await writeDbContext.BranchCustomers.SingleAsync(
                customer => customer.BranchId == branchId && customer.CustomerId == customerId,
                cancellationToken);
            return (winner, profile);
        }
    }

    public async Task ArchiveCustomerAsync(
        Guid branchId,
        Guid customerId,
        string managementKey,
        CancellationToken cancellationToken = default)
    {
        Authorize(branchId, managementKey);
        var relation = await writeDbContext.BranchCustomers.SingleOrDefaultAsync(
            customer => customer.BranchId == branchId && customer.CustomerId == customerId,
            cancellationToken);

        if (relation is null) return;
        relation.Archive(timeProvider.GetUtcNow().UtcDateTime);
        await writeDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateBranchProfileAsync(
        Guid branchId,
        string managementKey,
        string? description,
        string? address,
        string? publicPhoneNumber,
        string? logoUrl,
        string? coverImageUrl,
        CancellationToken cancellationToken = default)
    {
        Authorize(branchId, managementKey);
        var branch = await writeDbContext.Branches.SingleOrDefaultAsync(
            candidate => candidate.Id == branchId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("The branch was not found.");

        branch.UpdatePublicProfile(
            description,
            address,
            publicPhoneNumber,
            logoUrl,
            coverImageUrl);
        await writeDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> CreateMenuAsync(
        Guid branchId,
        string managementKey,
        string name,
        bool publish,
        CancellationToken cancellationToken = default)
    {
        Authorize(branchId, managementKey);
        if (!await writeDbContext.Branches.AnyAsync(branch => branch.Id == branchId, cancellationToken))
            throw new ResourceNotFoundException("The branch was not found.");

        var menu = Menu.Create(branchId, name, timeProvider.GetUtcNow().UtcDateTime);
        if (publish) menu.Publish();
        writeDbContext.Menus.Add(menu);
        await writeDbContext.SaveChangesAsync(cancellationToken);
        return menu.Id;
    }

    public async Task<Guid> CreateMenuItemAsync(
        Guid branchId,
        Guid menuId,
        string managementKey,
        string name,
        string? description,
        string? categoryName,
        string? imageUrl,
        decimal price,
        bool isAvailable,
        int displayOrder,
        CancellationToken cancellationToken = default)
    {
        Authorize(branchId, managementKey);
        await EnsureMenuBelongsToBranchAsync(branchId, menuId, cancellationToken);

        var item = MenuItem.Create(
            menuId,
            name,
            price,
            timeProvider.GetUtcNow().UtcDateTime,
            categoryName,
            displayOrder);
        item.Update(
            name, description, categoryName, imageUrl, price, isAvailable, displayOrder);
        writeDbContext.MenuItems.Add(item);
        await writeDbContext.SaveChangesAsync(cancellationToken);
        return item.Id;
    }

    public async Task UpdateMenuItemAsync(
        Guid branchId,
        Guid menuId,
        Guid itemId,
        string managementKey,
        string name,
        string? description,
        string? categoryName,
        string? imageUrl,
        decimal price,
        bool isAvailable,
        int displayOrder,
        CancellationToken cancellationToken = default)
    {
        Authorize(branchId, managementKey);
        await EnsureMenuBelongsToBranchAsync(branchId, menuId, cancellationToken);
        var item = await writeDbContext.MenuItems.SingleOrDefaultAsync(
            candidate => candidate.Id == itemId && candidate.MenuId == menuId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("The menu item was not found.");

        item.Update(
            name, description, categoryName, imageUrl, price, isAvailable, displayOrder);
        await writeDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> CreateVenueEventAsync(
        Guid branchId,
        string managementKey,
        string title,
        string? description,
        string? imageUrl,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        bool isPublished,
        CancellationToken cancellationToken = default)
    {
        Authorize(branchId, managementKey);
        if (!await writeDbContext.Branches.AnyAsync(branch => branch.Id == branchId, cancellationToken))
            throw new ResourceNotFoundException("The branch was not found.");

        var venueEvent = VenueEvent.Create(
            branchId,
            title,
            startsAtUtc,
            endsAtUtc,
            timeProvider.GetUtcNow().UtcDateTime);
        venueEvent.Update(
            title, description, imageUrl, startsAtUtc, endsAtUtc, isPublished);
        writeDbContext.VenueEvents.Add(venueEvent);
        await writeDbContext.SaveChangesAsync(cancellationToken);
        return venueEvent.Id;
    }

    public async Task UpdateVenueEventAsync(
        Guid branchId,
        Guid eventId,
        string managementKey,
        string title,
        string? description,
        string? imageUrl,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        bool isPublished,
        CancellationToken cancellationToken = default)
    {
        Authorize(branchId, managementKey);
        var venueEvent = await writeDbContext.VenueEvents.SingleOrDefaultAsync(
            candidate => candidate.Id == eventId && candidate.BranchId == branchId,
            cancellationToken)
            ?? throw new ResourceNotFoundException("The event was not found.");

        venueEvent.Update(
            title, description, imageUrl, startsAtUtc, endsAtUtc, isPublished);
        await writeDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureMenuBelongsToBranchAsync(
        Guid branchId,
        Guid menuId,
        CancellationToken cancellationToken)
    {
        if (!await writeDbContext.Menus.AnyAsync(
                menu => menu.Id == menuId && menu.BranchId == branchId,
                cancellationToken))
            throw new ResourceNotFoundException("The menu was not found.");
    }

    private void Authorize(Guid branchId, string managementKey)
    {
        var configuredKeyHash = configuration[$"OwnerAccess:BranchKeyHashes:{branchId:D}"];
        if (string.IsNullOrWhiteSpace(configuredKeyHash) ||
            string.IsNullOrWhiteSpace(managementKey) ||
            !TokenHashing.FixedTimeEquals(configuredKeyHash, TokenHashing.Hash(managementKey)))
            throw new RequestUnauthorizedException("The branch management key is invalid.");
    }

}
