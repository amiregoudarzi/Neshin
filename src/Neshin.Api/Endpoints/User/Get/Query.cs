using FastEndpoints;
using Neshin.Application.Abstractions.Persistence;
using Neshin.Application.Common;

namespace Neshin.Api.Endpoints.User.Get;

internal sealed record Query : ICommand<Query.Response>
{
    public Guid UserId { get; init; }

    internal sealed record Response(
        Guid Id,
        bool IsPhoneNumberVerified,
        DateTime CreatedAtUtc);

    private sealed class Handler(IUserReadRepository userRepository)
        : ICommandHandler<Query, Response>
    {
        public async Task<Response> ExecuteAsync(Query query, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);

            return user is null
                ? throw new ResourceNotFoundException("The user was not found.")
                : new Response(user.Id, user.IsPhoneNumberVerified, user.CreatedAtUtc);
        }
    }
}
