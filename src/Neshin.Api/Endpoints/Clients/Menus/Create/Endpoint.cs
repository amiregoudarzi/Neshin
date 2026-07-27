using FastEndpoints;

namespace Neshin.Api.Endpoints.Clients.Menus.Create;

file sealed class Endpoint : Endpoint<Command, Command.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/clients/cafes/{cafeId}/menus");
        Version(1);
        AllowAnonymous();
        Options(builder => builder.RequireRateLimiting("owner"));
    }

    public override async Task HandleAsync(Command req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct);
        await Send.ResponseAsync(response, StatusCodes.Status201Created, ct);
    }

    private sealed class EndpointSummary : Summary<Endpoint>
    {
        public EndpointSummary()
        {
            Summary = "Create a cafe menu";
            Description = "Creates a menu and its items. Each cafe can have at most five menus.";
            Response<Command.Response>(201, "Menu created");
            Response(409, "The five-menu limit was reached");
        }
    }
}
