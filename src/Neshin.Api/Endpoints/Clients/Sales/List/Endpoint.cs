using FastEndpoints;

namespace Neshin.Api.Endpoints.Clients.Sales.List;

file sealed class Endpoint : Endpoint<Query, IReadOnlyList<Query.Response>>
{
    public override void Configure()
    {
        Get("/api/{version}/clients/cafes/{cafeId}/sales");
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
            Summary = "Filter cafe selling history";
            Description = "Filters accepted sales by UTC date range, customer, and menu item.";
            Response<IReadOnlyList<Query.Response>>(200, "Selling history");
        }
    }
}
