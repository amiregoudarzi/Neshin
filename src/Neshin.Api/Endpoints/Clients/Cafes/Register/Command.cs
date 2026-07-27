using FastEndpoints;

namespace Neshin.Api.Endpoints.Clients.Cafes.Register;

internal sealed record Command : ICommand<Command.Response>
{
    public string ClientName { get; init; } = string.Empty;
    public string CafeName { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string? Description { get; init; }
    public string? Address { get; init; }
    public string? PublicPhoneNumber { get; init; }
    public IReadOnlyList<string>? PhotoUrls { get; init; }

    internal sealed record Response(Guid ClientId, Guid CafeId, string ManagementKey);

    private sealed class Handler(IClientExperienceRepository repository)
        : ICommandHandler<Command, Response>
    {
        public async Task<Response> ExecuteAsync(Command command, CancellationToken ct)
        {
            var result = await repository.RegisterCafeAsync(
                command.ClientName,
                command.CafeName,
                command.Latitude,
                command.Longitude,
                command.Description,
                command.Address,
                command.PublicPhoneNumber,
                command.PhotoUrls,
                ct);
            return new Response(result.ClientId, result.CafeId, result.ManagementKey);
        }
    }
}
