using FastEndpoints;

namespace Neshin.Api.Endpoints.Clients.Customers.List;

file sealed class Endpoint : Endpoint<Query, IReadOnlyList<Query.Response>>
{
    public override void Configure()
    {
        Get("/api/{version}/clients/cafes/{cafeId}/customers");
        Version(1);
        AllowAnonymous();
        Options(builder => builder.RequireRateLimiting("owner"));
    }

    public override async Task HandleAsync(Query req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct);
        await Send.OkAsync(response, ct);
    }

    private sealed class EndpointSummary : Summary<Endpoint>
    {
        public EndpointSummary()
        {
            Summary = "List cafe customers";
            Description = "Lists customers whose orders were accepted by this cafe.";
            Response<IReadOnlyList<Query.Response>>(200, "Customer list");
        }
    }
}
