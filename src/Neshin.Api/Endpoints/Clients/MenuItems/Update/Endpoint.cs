using FastEndpoints;

namespace Neshin.Api.Endpoints.Clients.MenuItems.Update;

file sealed class Endpoint : Endpoint<Command>
{
    public override void Configure()
    {
        Put("/api/{version}/clients/cafes/{cafeId}/menus/{menuId}/items/{itemId}");
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
            Description = "Updates the title, caption, category, photo, price, availability, and display order.";
            Response(204, "Menu item updated");
        }
    }
}
