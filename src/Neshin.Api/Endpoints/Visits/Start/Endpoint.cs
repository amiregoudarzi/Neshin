using FastEndpoints;

namespace Neshin.Api.Endpoints.Visits.Start;

file sealed class Endpoint : Endpoint<Command, Command.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/customer/visits");
        Version(1);
        AllowAnonymous();
        Options(builder => builder.RequireRateLimiting("customer-write"));
    }

    public override async Task HandleAsync(Command req, CancellationToken ct)
    {
        var response = await req.ExecuteAsync(ct);
        await Send.OkAsync(response, ct);
    }

    private sealed class EndpointSummary : Summary<Endpoint>
    {
        public EndpointSummary()
        {
            Summary = "Start or refresh a venue visit";
            Description = "Records presence without storing or exposing exact device coordinates.";
            Response<Command.Response>(200, "Successful");
        }
    }
}
