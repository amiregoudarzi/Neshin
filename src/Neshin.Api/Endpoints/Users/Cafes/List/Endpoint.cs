using FastEndpoints;

namespace Neshin.Api.Endpoints.Users.Cafes.List;

file sealed class Endpoint : Endpoint<Query, IReadOnlyList<Query.Response>>
{
    public override void Configure()
    {
        Get("/api/{version}/users/cafes");
        Version(1);
        AllowAnonymous();
        Options(builder => builder.RequireRateLimiting("discovery"));
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
            Summary = "List registered cafes";
            Description = "Lists active cafes and optionally filters and sorts them by device location.";
            Response<IReadOnlyList<Query.Response>>(200, "Cafe list");
        }
    }
}
