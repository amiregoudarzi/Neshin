using FastEndpoints;

namespace Neshin.Api.Endpoints.OwnerContent.UpdateMenuItem;

file sealed class Endpoint : Endpoint<Command>
{
    public override void Configure()
    {
        Put("/api/{version}/owner/branches/{branchId:guid}/menus/{menuId:guid}/items/{itemId:guid}");
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
            Summary = "Update a menu item";
            Description = "Updates an existing menu item.";
            Response(204, "Menu item updated");
        }
    }
}
