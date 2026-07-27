namespace Neshin.Api.Http;

internal interface IRequestContext
{
    public string CustomerSessionToken { get; }
    public string ManagementKey { get; }
    public string IdempotencyKey { get; }
    public void SetNoStore();
    public void SetPublicStorefrontCache();
    public void SetCustomerSessionCookie(string accessToken, DateTime expiresAtUtc);
}
