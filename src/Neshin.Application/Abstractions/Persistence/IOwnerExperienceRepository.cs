using Neshin.Domain.Customers;
using Neshin.Domain.Ordering;

namespace Neshin.Application.Abstractions.Persistence;

public interface IOwnerExperienceRepository
{
    public Task<IReadOnlyList<Order>> GetOrdersAsync(
        Guid branchId, string managementKey, string? status, CancellationToken cancellationToken = default);

    public Task<Order> ChangeOrderStatusAsync(
        Guid branchId, Guid orderId, string managementKey, string action, string? reason, int expectedVersion,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<(VenueVisit Visit, bool HasOpenOrder)>> GetActiveVisitsAsync(
        Guid branchId, string managementKey, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<(BranchCustomer Relation, CustomerProfile Profile)>> GetCustomersAsync(
        Guid branchId, string managementKey, bool includeArchived,
        CancellationToken cancellationToken = default);

    public Task<(BranchCustomer Relation, CustomerProfile Profile)> AddCustomerAsync(
        Guid branchId, Guid customerId, string managementKey, string? notes,
        CancellationToken cancellationToken = default);

    public Task ArchiveCustomerAsync(
        Guid branchId, Guid customerId, string managementKey,
        CancellationToken cancellationToken = default);

    public Task UpdateBranchProfileAsync(
        Guid branchId, string managementKey, string? description, string? address,
        string? publicPhoneNumber, string? logoUrl, string? coverImageUrl,
        CancellationToken cancellationToken = default);

    public Task<Guid> CreateMenuAsync(
        Guid branchId, string managementKey, string name, bool publish,
        CancellationToken cancellationToken = default);

    public Task<Guid> CreateMenuItemAsync(
        Guid branchId, Guid menuId, string managementKey, string name, string? description,
        string? categoryName, string? imageUrl, decimal price, bool isAvailable, int displayOrder,
        CancellationToken cancellationToken = default);

    public Task UpdateMenuItemAsync(
        Guid branchId, Guid menuId, Guid itemId, string managementKey, string name, string? description,
        string? categoryName, string? imageUrl, decimal price, bool isAvailable, int displayOrder,
        CancellationToken cancellationToken = default);

    public Task<Guid> CreateVenueEventAsync(
        Guid branchId, string managementKey, string title, string? description, string? imageUrl,
        DateTime startsAtUtc, DateTime endsAtUtc, bool isPublished,
        CancellationToken cancellationToken = default);

    public Task UpdateVenueEventAsync(
        Guid branchId, Guid eventId, string managementKey, string title, string? description,
        string? imageUrl, DateTime startsAtUtc, DateTime endsAtUtc, bool isPublished,
        CancellationToken cancellationToken = default);
}
