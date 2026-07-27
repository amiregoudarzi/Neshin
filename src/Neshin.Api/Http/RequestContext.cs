namespace Neshin.Api.Http;

internal sealed class RequestContext(IHttpContextAccessor accessor) : IRequestContext
{
    private HttpContext HttpContext =>
        accessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request exists.");

    public string CustomerSessionToken
    {
        get
        {
            var token = HttpContext.Request.Headers["X-Customer-Session"].ToString();
            return string.IsNullOrWhiteSpace(token)
                ? HttpContext.Request.Cookies["neshin_session"] ?? string.Empty
                : token;
        }
    }

    public string ManagementKey =>
        HttpContext.Request.Headers["X-Branch-Management-Key"].ToString();

    public string IdempotencyKey =>
        HttpContext.Request.Headers["Idempotency-Key"].ToString();

    public void SetNoStore() =>
        HttpContext.Response.Headers.CacheControl = "no-store";

    public void SetPublicStorefrontCache() =>
        HttpContext.Response.Headers.CacheControl = "public,max-age=30,stale-while-revalidate=120";

    public void SetCustomerSessionCookie(string accessToken, DateTime expiresAtUtc) =>
        HttpContext.Response.Cookies.Append(
            "neshin_session",
            accessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = expiresAtUtc,
                Path = "/api/v1"
            });
}
