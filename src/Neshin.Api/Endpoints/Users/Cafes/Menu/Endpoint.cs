using FastEndpoints;

namespace Neshin.Api.Endpoints.Users.Cafes.Menu;

file sealed class Endpoint : Endpoint<Query, Query.Response>
{
    public override void Configure()
    {
        Get("/api/{version}/users/cafes/{cafeId}/menus");
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
            Summary = "Get cafe menus";
            Description = "Returns all published menus and available items for a cafe.";
            Response<Query.Response>(200, "Cafe menus");
            Response(404, "Cafe not found");
        }
    }
}
