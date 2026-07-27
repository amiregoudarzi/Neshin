using FastEndpoints;

namespace Neshin.Api.Endpoints.Discovery.Resolve;

file sealed class Endpoint : Endpoint<Query, Query.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/discovery/resolve");
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
            Summary = "Find the best matching venue";
            Description = "Returns a list-shaped result so co-located venues can be presented for selection.";
            Response<Query.Response>(200, "Successful");
        }
    }
}
