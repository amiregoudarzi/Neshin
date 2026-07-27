using FastEndpoints;

namespace Neshin.Api.Endpoints.Storefront.Get;

file sealed class Endpoint : Endpoint<Query, Query.Response>
{
    public override void Configure()
    {
        Get("/api/{version}/public/branches/{branchId:guid}");
        Version(1);
        AllowAnonymous();
        Options(builder => builder.RequireRateLimiting("public-read"));
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
            Summary = "Get a venue storefront";
            Description = "Returns the public branch profile, published menus/items, and upcoming events.";
            Response<Query.Response>(200, "Successful");
            Response(404, "Storefront not found");
        }
    }
}
