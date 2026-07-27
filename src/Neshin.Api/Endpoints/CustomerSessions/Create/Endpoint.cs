using FastEndpoints;

namespace Neshin.Api.Endpoints.CustomerSessions.Create;


file sealed class Endpoint : EndpointWithoutRequest<Command.Response>
{
    public override void Configure()
    {
        Post("/api/{version}/customer-sessions");
        Version(1);
        AllowAnonymous();
        Options(builder => builder.RequireRateLimiting("customer-session"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = await new Command().ExecuteAsync(ct);
        await Send.ResponseAsync(response, StatusCodes.Status201Created, ct);
    }

    private sealed class EndpointSummary : Summary<Endpoint>
    {
        public EndpointSummary()
        {
            Summary = "Create an anonymous customer session";
            Description = "Issues an opaque guest token. Phone number and OTP are not required.";
            Response<Command.Response>(201, "Session created");
        }
    }
}
