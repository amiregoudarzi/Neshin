using FastEndpoints;

namespace Neshin.Api.Endpoints.Users.OrderHistory;

file sealed class Endpoint : EndpointWithoutRequest<IReadOnlyList<Query.Response>>
{
    public override void Configure()
    {
        Get("/api/{version}/users/orders");
        Version(1);
        AllowAnonymous();
        Options(builder => builder.RequireRateLimiting("public-read"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = await new Query().ExecuteAsync(ct);
        await Send.OkAsync(response, ct);
    }

    private sealed class EndpointSummary : Summary<Endpoint>
    {
        public EndpointSummary()
        {
            Summary = "Get user order history";
            Description = "Returns only orders that a cafe accepted.";
            Response<IReadOnlyList<Query.Response>>(200, "Accepted order history");
        }
    }
}
