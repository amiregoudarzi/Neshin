using FastEndpoints;

namespace Neshin.Api.Endpoints.Clients.Cafes.Register;

file sealed class Endpoint : Endpoint<Command, Command.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/clients/cafes");
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
            Summary = "Register a cafe";
            Description = "Creates an active client and cafe and returns its one-time management key.";
            Response<Command.Response>(201, "Cafe registered");
        }
    }
}
