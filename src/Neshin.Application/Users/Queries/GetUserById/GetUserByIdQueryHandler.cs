using Neshin.Application.Abstractions.Messaging;
using Neshin.Application.Abstractions.Persistence;

namespace Neshin.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(IUserReadRepository userRepository)
    : IQueryHandler<GetUserByIdQuery, UserReadModel?>
{
    public Task<UserReadModel?> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken) =>
        userRepository.GetByIdAsync(query.UserId, cancellationToken);
}
