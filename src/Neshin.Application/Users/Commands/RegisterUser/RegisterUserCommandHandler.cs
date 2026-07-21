using Neshin.Application.Abstractions.Authentication;
using Neshin.Application.Abstractions.Messaging;
using Neshin.Application.Abstractions.Persistence;
using Neshin.Domain.Common;
using Neshin.Domain.Identity;

namespace Neshin.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserWriteRepository userRepository,
    IUnitOfWork unitOfWork,
    IOtpVerifier otpVerifier,
    TimeProvider timeProvider) : ICommandHandler<RegisterUserCommand, RegisterUserResult>
{
    public async Task<RegisterUserResult> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedPhoneNumber = User.NormalizePhoneNumber(command.PhoneNumber);

        if (!await otpVerifier.VerifyAsync(normalizedPhoneNumber, command.OtpCode, cancellationToken))
        {
            throw new DomainException("The OTP code is invalid or expired.");
        }

        // The write repository is intentional: this uniqueness check is consistency-critical.
        var existingUser = await userRepository.GetByPhoneNumberAsync(normalizedPhoneNumber, cancellationToken);
        if (existingUser is not null)
        {
            if (!existingUser.IsPhoneNumberVerified)
            {
                existingUser.VerifyPhoneNumber(timeProvider.GetUtcNow());
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return new RegisterUserResult(existingUser.Id, false);
        }

        var user = User.Create(normalizedPhoneNumber, timeProvider.GetUtcNow());
        user.VerifyPhoneNumber(timeProvider.GetUtcNow());
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterUserResult(user.Id, true);
    }
}
