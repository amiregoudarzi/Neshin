namespace Neshin.Application.Abstractions.Authentication;

public interface IOtpVerifier
{
    public Task<bool> VerifyAsync(string phoneNumber, string otpCode, CancellationToken cancellationToken = default);
}
