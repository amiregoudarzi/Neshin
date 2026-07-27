using Microsoft.EntityFrameworkCore;
using Neshin.Application.Abstractions.Authentication;
using Neshin.Application.Abstractions.Persistence;
using Neshin.Application.Common;
using Neshin.Domain.Common;
using Neshin.Domain.Customers;
using Neshin.Domain.Identity;
using Neshin.Domain.Ordering;
using Neshin.Infrastructure.CustomerExperience;
using Npgsql;

namespace Neshin.Infrastructure.Persistence.Repositories;

public sealed class UserExperienceRepository(
    NeshinWriteDbContext writeDbContext,
    NeshinReadDbContext readDbContext,
    IOtpVerifier otpVerifier,
    TimeProvider timeProvider) : IUserExperienceRepository
{
    private static readonly TimeSpan LoginLifetime = TimeSpan.FromDays(7);
    private static readonly OrderStatus[] AcceptedStatuses =
        [OrderStatus.Accepted, OrderStatus.Preparing, OrderStatus.Ready, OrderStatus.Completed];

    public async Task<UserSessionResult> QuickSignUpAsync(
        string phoneNumber,
        string name,
        string? otpCode,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var normalizedPhone = User.NormalizePhoneNumber(phoneNumber);
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required.");

        var user = await writeDbContext.Users.SingleOrDefaultAsync(
            candidate => candidate.PhoneNumber == normalizedPhone,
            cancellationToken);
        var isNewUser = user is null;
        var otpWasRequired = user is not null &&
                             (user.PhoneNumberVerifiedAtUtc is null ||
                              user.PhoneNumberVerifiedAtUtc <= now.Subtract(LoginLifetime));

        if (otpWasRequired)
        {
            if (string.IsNullOrWhiteSpace(otpCode) ||
                !await otpVerifier.VerifyAsync(normalizedPhone, otpCode, cancellationToken))
                throw new RequestUnauthorizedException("A valid OTP is required because the weekly login expired.");
            user!.VerifyPhoneNumber(now);
        }

        if (user is null)
        {
            user = User.Create(normalizedPhone, now);
            user.VerifyPhoneNumber(now);
            writeDbContext.Users.Add(user);
        }

        var customer = await writeDbContext.CustomerProfiles.SingleOrDefaultAsync(
            candidate => candidate.UserId == user.Id,
            cancellationToken);
        if (customer is null)
        {
            customer = CustomerProfile.CreateRegistered(user.Id, name, normalizedPhone, now);
            writeDbContext.CustomerProfiles.Add(customer);
        }
        else
        {
            customer.UpdateRegisteredContact(name, normalizedPhone);
        }

        var accessToken = TokenHashing.CreateToken();
        var session = CustomerSession.Create(
            customer.Id,
            TokenHashing.Hash(accessToken),
            now,
            now.Add(LoginLifetime));
        writeDbContext.CustomerSessions.Add(session);
        await writeDbContext.SaveChangesAsync(cancellationToken);

        return new UserSessionResult(
            user.Id,
            customer.Id,
            accessToken,
            session.ExpiresAtUtc,
            isNewUser,
            otpWasRequired);
    }

    public async Task<IReadOnlyList<(Order Order, string CafeName)>> GetAcceptedOrderHistoryAsync(
        string sessionToken,
        CancellationToken cancellationToken = default)
    {
        var customerId = await ResolveReadCustomerIdAsync(sessionToken, cancellationToken);
        var rows = await (
                from order in readDbContext.Orders.Include(order => order.Items)
                join cafe in readDbContext.Branches on order.BranchId equals cafe.Id
                where order.CustomerId == customerId && AcceptedStatuses.Contains(order.Status)
                orderby order.CreatedAtUtc descending
                select new { Order = order, CafeName = cafe.Name })
            .Take(100)
            .ToListAsync(cancellationToken);
        return rows.Select(row => (row.Order, row.CafeName)).ToList();
    }

    public async Task<IReadOnlyList<CafeSearchResult>> GetCafesAsync(
        bool nearbyOnly,
        decimal? latitude,
        decimal? longitude,
        decimal radiusMeters,
        CancellationToken cancellationToken = default)
    {
        if (nearbyOnly && (latitude is null || longitude is null))
            throw new DomainException("Latitude and longitude are required for nearby cafes.");
        if (radiusMeters is < 25 or > 50_000)
            throw new DomainException("Radius must be between 25 and 50000 meters.");

        var cafes = await (
                from cafe in readDbContext.Branches
                join client in readDbContext.Clients on cafe.ClientId equals client.Id
                where cafe.IsActive && client.IsActive
                orderby cafe.Name
                select cafe)
            .ToListAsync(cancellationToken);

        if (!nearbyOnly)
            return cafes.Select(cafe => new CafeSearchResult(cafe, null)).ToList();

        return cafes
            .Select(cafe => new CafeSearchResult(
                cafe,
                CalculateDistanceMeters(latitude!.Value, longitude!.Value, cafe.Latitude, cafe.Longitude)))
            .Where(result => result.DistanceMeters <= radiusMeters)
            .OrderBy(result => result.DistanceMeters)
            .Take(100)
            .ToList();
    }

    public async Task<(Domain.Clients.Branch Cafe, IReadOnlyList<Domain.Catalog.Menu> Menus, IReadOnlyList<Domain.Catalog.MenuItem> Items)?>
        GetCafeMenuAsync(Guid cafeId, CancellationToken cancellationToken = default)
    {
        var cafe = await readDbContext.Branches.SingleOrDefaultAsync(
            candidate => candidate.Id == cafeId && candidate.IsActive,
            cancellationToken);
        if (cafe is null) return null;

        var menus = await readDbContext.Menus
            .Where(menu => menu.BranchId == cafeId && menu.IsPublished)
            .OrderBy(menu => menu.Name)
            .ToListAsync(cancellationToken);
        var menuIds = menus.Select(menu => menu.Id).ToList();
        var items = await readDbContext.MenuItems
            .Where(item => menuIds.Contains(item.MenuId) && item.IsAvailable)
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.Name)
            .ToListAsync(cancellationToken);
        return (cafe, menus, items);
    }

    public async Task<Order> PlaceOrderAsync(
        string sessionToken,
        Guid cafeId,
        string idempotencyKey,
        IReadOnlyList<(Guid MenuItemId, int Quantity)> items,
        CancellationToken cancellationToken = default)
    {
        ValidateItems(items);
        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
            throw new DomainException("A valid idempotency key is required.");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var (session, customer) = await ResolveWriteSessionAsync(sessionToken, now, cancellationToken);
        var existing = await writeDbContext.Orders.Include(order => order.Items).SingleOrDefaultAsync(
            order => order.CustomerId == customer.Id && order.IdempotencyKey == idempotencyKey.Trim(),
            cancellationToken);
        if (existing is not null) return existing;

        var cafe = await writeDbContext.Branches.SingleOrDefaultAsync(
            branch => branch.Id == cafeId && branch.IsActive,
            cancellationToken) ?? throw new ResourceNotFoundException("The cafe was not found.");
        var selectedItems = await LoadSelectedItemsAsync(cafeId, items, cancellationToken);

        var order = Order.Create(
            cafeId,
            customer.Id,
            PaymentMethod.PayAtVenuePos,
            idempotencyKey.Trim(),
            cafe.AcceptsAppOrders,
            cafe.AllowsPayAtVenue,
            now,
            customer.UserId);
        foreach (var requested in items)
        {
            var item = selectedItems.Single(candidate => candidate.Id == requested.MenuItemId);
            order.AddItem(item.Id, item.Name, item.Price, requested.Quantity);
        }
        order.SetContact(customer.DisplayName, customer.ContactPhoneNumber, true);
        order.SubmitForPayment(now);
        session.Touch(now);
        writeDbContext.Orders.Add(order);

        try
        {
            await writeDbContext.SaveChangesAsync(cancellationToken);
            return order;
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return await writeDbContext.Orders.Include(candidate => candidate.Items).SingleAsync(
                candidate => candidate.CustomerId == customer.Id &&
                             candidate.IdempotencyKey == idempotencyKey.Trim(),
                cancellationToken);
        }
    }

    public async Task<Invoice> CreateInvoiceAsync(
        string sessionToken,
        Guid cafeId,
        IReadOnlyList<(Guid MenuItemId, int Quantity)> items,
        CancellationToken cancellationToken = default)
    {
        ValidateItems(items);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var (session, customer) = await ResolveWriteSessionAsync(sessionToken, now, cancellationToken);
        var selectedItems = await LoadSelectedItemsAsync(cafeId, items, cancellationToken);
        var invoice = Invoice.Create(cafeId, customer.Id, now);
        foreach (var requested in items)
        {
            var item = selectedItems.Single(candidate => candidate.Id == requested.MenuItemId);
            invoice.AddItem(item.Id, item.Name, item.Price, requested.Quantity);
        }
        session.Touch(now);
        writeDbContext.Invoices.Add(invoice);
        await writeDbContext.SaveChangesAsync(cancellationToken);
        return invoice;
    }

    private async Task<List<Domain.Catalog.MenuItem>> LoadSelectedItemsAsync(
        Guid cafeId,
        IReadOnlyList<(Guid MenuItemId, int Quantity)> requestedItems,
        CancellationToken cancellationToken)
    {
        var ids = requestedItems.Select(item => item.MenuItemId).ToList();
        var items = await (
                from item in writeDbContext.MenuItems
                join menu in writeDbContext.Menus on item.MenuId equals menu.Id
                where ids.Contains(item.Id) && item.IsAvailable && menu.IsPublished && menu.BranchId == cafeId
                select item)
            .ToListAsync(cancellationToken);
        if (items.Count != ids.Count)
            throw new DomainException("One or more menu items are unavailable for this cafe.");
        return items;
    }

    private async Task<(CustomerSession Session, CustomerProfile Customer)> ResolveWriteSessionAsync(
        string token,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new RequestUnauthorizedException("A user session token is required.");
        var hash = TokenHashing.Hash(token);
        var session = await writeDbContext.CustomerSessions.SingleOrDefaultAsync(
            candidate => candidate.TokenHash == hash,
            cancellationToken);
        if (session is null || !session.IsValidAt(now))
            throw new RequestUnauthorizedException("The user session is invalid or expired.");
        var customer = await writeDbContext.CustomerProfiles.SingleAsync(
            candidate => candidate.Id == session.CustomerId && candidate.UserId != null,
            cancellationToken);
        return (session, customer);
    }

    private async Task<Guid> ResolveReadCustomerIdAsync(
        string token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new RequestUnauthorizedException("A user session token is required.");
        var hash = TokenHashing.Hash(token);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var customerId = await (
                from session in readDbContext.CustomerSessions
                join customer in readDbContext.CustomerProfiles on session.CustomerId equals customer.Id
                where session.TokenHash == hash &&
                      session.RevokedAtUtc == null &&
                      session.ExpiresAtUtc > now &&
                      customer.UserId != null
                select (Guid?)customer.Id)
            .SingleOrDefaultAsync(cancellationToken);
        return customerId ?? throw new RequestUnauthorizedException("The user session is invalid or expired.");
    }

    private static void ValidateItems(IReadOnlyList<(Guid MenuItemId, int Quantity)> items)
    {
        if (items.Count is < 1 or > 30)
            throw new DomainException("Between 1 and 30 items are required.");
        if (items.Any(item => item.MenuItemId == Guid.Empty || item.Quantity is < 1 or > 20))
            throw new DomainException("Each item needs a valid id and quantity between 1 and 20.");
        if (items.Select(item => item.MenuItemId).Distinct().Count() != items.Count)
            throw new DomainException("Duplicate menu items must be combined.");
    }

    private static decimal CalculateDistanceMeters(
        decimal latitude,
        decimal longitude,
        decimal cafeLatitude,
        decimal cafeLongitude)
    {
        if (latitude is < -90 or > 90 || cafeLatitude is < -90 or > 90)
            throw new DomainException("Latitude is out of range.");
        if (longitude is < -180 or > 180 || cafeLongitude is < -180 or > 180)
            throw new DomainException("Longitude is out of range.");
        const double earthRadius = 6_371_000;
        static double Radians(decimal value) => (double)value * Math.PI / 180;
        var lat1 = Radians(latitude);
        var lat2 = Radians(cafeLatitude);
        var latDelta = Radians(cafeLatitude - latitude);
        var lonDelta = Radians(cafeLongitude - longitude);
        var value = Math.Sin(latDelta / 2) * Math.Sin(latDelta / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(lonDelta / 2) * Math.Sin(lonDelta / 2);
        return Math.Round((decimal)(earthRadius * 2 * Math.Atan2(Math.Sqrt(value), Math.Sqrt(1 - value))), 2);
    }
}
