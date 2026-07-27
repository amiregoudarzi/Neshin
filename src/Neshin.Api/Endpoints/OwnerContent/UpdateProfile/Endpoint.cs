using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerContent.UpdateProfile;

file sealed class Endpoint : Endpoint<Command>
{
    public override void Configure()
    {
        Put("/api/{version}/owner/branches/{branchId:guid}/profile");
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
            Summary = "Update the public branch profile";
            Description = "Updates the public information displayed for a branch.";
            Response(204, "Branch profile updated");
        }
    }
}
