namespace Prodify.Api.Security;

// Browser safety headers on every API response. The API only returns JSON and product photos,
// so pages may not frame it, run scripts from it or guess a different content type.
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        // Swagger (Development only) is a real page with scripts, so it is left out.
        if (!context.Request.Path.StartsWithSegments("/swagger"))
            headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";

        return _next(context);
    }
}
