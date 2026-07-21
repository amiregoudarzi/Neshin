using Neshin.Application.Abstractions.Messaging;
using Neshin.Application.Abstractions.Persistence;

namespace Neshin.Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserReadModel?>;
