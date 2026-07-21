using Neshin.Application.Abstractions.Messaging;

namespace Neshin.Application.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(string PhoneNumber, string OtpCode) : ICommand<RegisterUserResult>;

public sealed record RegisterUserResult(Guid UserId, bool IsNewUser);
