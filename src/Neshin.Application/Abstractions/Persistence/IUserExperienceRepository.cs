using Neshin.Domain.Catalog;
using Neshin.Domain.Clients;
using Neshin.Domain.Customers;
using Neshin.Domain.Ordering;

namespace Neshin.Application.Abstractions.Persistence;

public interface IUserExperienceRepository
{
    public Task<UserSessionResult> QuickSignUpAsync(
        string phoneNumber,
        string name,
        string? otpCode,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<(Order Order, string CafeName)>> GetAcceptedOrderHistoryAsync(
        string sessionToken,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<CafeSearchResult>> GetCafesAsync(
        bool nearbyOnly,
        decimal? latitude,
        decimal? longitude,
        decimal radiusMeters,
        CancellationToken cancellationToken = default);

    public Task<(Branch Cafe, IReadOnlyList<Menu> Menus, IReadOnlyList<MenuItem> Items)?> GetCafeMenuAsync(
        Guid cafeId,
        CancellationToken cancellationToken = default);

    public Task<Order> PlaceOrderAsync(
        string sessionToken,
        Guid cafeId,
        string idempotencyKey,
        IReadOnlyList<(Guid MenuItemId, int Quantity)> items,
        CancellationToken cancellationToken = default);

    public Task<Invoice> CreateInvoiceAsync(
        string sessionToken,
        Guid cafeId,
        IReadOnlyList<(Guid MenuItemId, int Quantity)> items,
        CancellationToken cancellationToken = default);
}

public sealed record UserSessionResult(
    Guid UserId,
    Guid CustomerId,
    string AccessToken,
    DateTime ExpiresAtUtc,
    bool IsNewUser,
    bool OtpWasRequired);

public sealed record CafeSearchResult(Branch Cafe, decimal? DistanceMeters);
