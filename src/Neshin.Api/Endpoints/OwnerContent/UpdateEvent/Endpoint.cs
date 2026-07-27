using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerContent.UpdateEvent;

file sealed class Endpoint : Endpoint<Command>
{
    public override void Configure()
    {
        Put("/api/{version}/owner/branches/{branchId:guid}/events/{eventId:guid}");
        Version(1);
        AllowAnonymous();
        Options(builder => builder.RequireRateLimiting("owner"));
    }

    public override async Task HandleAsync(Command req, CancellationToken ct)
    {
        await req.ExecuteAsync(ct);
        await Send.NoContentAsync(ct);
    }

    private sealed class EndpointSummary : Summary<Endpoint>
    {
        public EndpointSummary()
        {
            Summary = "Update a venue event";
            Description = "Updates an existing venue event.";
            Response(204, "Event updated");
        }
    }
}
