namespace Neshin.Application.Abstractions.Authentication;

public interface IOtpVerifier
{
    Task<bool> VerifyAsync(string phoneNumber, string otpCode, CancellationToken cancellationToken = default);
}
