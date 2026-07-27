using FastEndpoints;

namespace Neshin.Api.Endpoints.Clients.Customers.List;

internal sealed record Query : ICommand<IReadOnlyList<Query.Response>>
{
    public Guid CafeId { get; init; }

    internal sealed record Response
    {
        public Guid CustomerId { get; init; }
        public string? Name { get; init; }
        public string? PhoneNumber { get; init; }
        public DateTime AddedAtUtc { get; init; }
    }

    private sealed class Handler(
        IClientExperienceRepository repository,
        IRequestContext requestContext) : ICommandHandler<Query, IReadOnlyList<Response>>
    {
        public async Task<IReadOnlyList<Response>> ExecuteAsync(Query query, CancellationToken ct)
        {
            requestContext.SetNoStore();
            var customers = await repository.GetCustomersAsync(
                query.CafeId,
                requestContext.ManagementKey,
                ct);
            return customers.Select(result => new Response
            {
                CustomerId = result.Customer.Id,
                Name = result.Customer.DisplayName,
                PhoneNumber = result.Relation.ContactPhoneNumber,
                AddedAtUtc = result.Relation.AddedAtUtc
            }).ToList();
        }
    }
}
