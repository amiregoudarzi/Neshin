using Neshin.Application.Abstractions.Authentication;
using Neshin.Application.Abstractions.Persistence;
using Neshin.Application.Users.Commands.RegisterUser;
using Neshin.Domain.Identity;

namespace Neshin.Application.UnitTests.Users;

public sealed class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidOtp_CreatesVerifiedUser()
    {
        var repository = new UserWriteRepositoryFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new RegisterUserCommandHandler(
            repository,
            unitOfWork,
            new OtpVerifierFake(isValid: true),
            TimeProvider.System);

        var result = await handler.HandleAsync(
            new RegisterUserCommand("+989121234567", "12345"),
            CancellationToken.None);

        Assert.True(result.IsNewUser);
        Assert.NotNull(repository.AddedUser);
        Assert.True(repository.AddedUser.IsPhoneNumberVerified);
        Assert.Equal("09121234567", repository.AddedUser.PhoneNumber);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    private sealed class UserWriteRepositoryFake : IUserWriteRepository
    {
        public User? AddedUser { get; private set; }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<User?>(null);

        public Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default) =>
            Task.FromResult<User?>(null);

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            AddedUser = user;
            return Task.CompletedTask;
        }
    }

    private sealed class UnitOfWorkFake : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class OtpVerifierFake(bool isValid) : IOtpVerifier
    {
        public Task<bool> VerifyAsync(
            string phoneNumber,
            string otpCode,
            CancellationToken cancellationToken = default) => Task.FromResult(isValid);
    }
}
