using FastEndpoints;

namespace Neshin.Api.Endpoints.Users.Payments.Pay;

file sealed class Endpoint : Endpoint<Command, Command.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/users/payments/{invoiceId}");
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
            Summary = "Pay an invoice";
            Description = "Reserved for the future third-party payment provider.";
            Response<Command.Response>(200, "Payment completed");
            Response(501, "Payment provider is not configured");
        }
    }
}
