using FastEndpoints;

namespace Neshin.Api.Endpoints.User.Get;

file sealed class Endpoint : Endpoint<Query, Query.Response>
{
    public override void Configure()
    {
        Get("/api/{version}/users/{userId:guid}");
        Version(1);
        AllowAnonymous();
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
            Summary = "Get User By Id";
            Description = "Returns the requested user when it exists.";
            Response<Query.Response>(200, "Successful");
            Response(404, "User not found");
        }
    }
}
