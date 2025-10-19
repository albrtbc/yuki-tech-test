namespace Yuki.Blog.Api.Middleware;

/// <summary>
/// Middleware that adds security headers to HTTP responses.
/// </summary>
/// <remarks>
/// This middleware adds the following security headers:
/// - Strict-Transport-Security (HSTS): Enforces HTTPS connections
/// - X-Content-Type-Options: Prevents MIME type sniffing
/// - X-Frame-Options: Prevents clickjacking attacks
/// - X-XSS-Protection: Enables browser XSS filtering
/// - Referrer-Policy: Controls referrer information
/// - Content-Security-Policy: Restricts resource loading
/// - Permissions-Policy: Controls browser features
/// </remarks>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(
        RequestDelegate next,
        ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers to response
        var headers = context.Response.Headers;

        // Strict-Transport-Security (HSTS)
        // Tells browsers to only access the site via HTTPS for the next year
        // includeSubDomains: Apply to all subdomains
        // preload: Allow inclusion in browser HSTS preload lists
        if (!headers.ContainsKey("Strict-Transport-Security"))
        {
            headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        }

        // X-Content-Type-Options
        // Prevents MIME type sniffing attacks
        // Browser will strictly follow the Content-Type header
        if (!headers.ContainsKey("X-Content-Type-Options"))
        {
            headers.Append("X-Content-Type-Options", "nosniff");
        }

        // X-Frame-Options
        // Prevents clickjacking by disallowing the page to be embedded in frames/iframes
        if (!headers.ContainsKey("X-Frame-Options"))
        {
            headers.Append("X-Frame-Options", "DENY");
        }

        // X-XSS-Protection
        // Enables browser's XSS filter to block detected attacks
        // mode=block: Block the page entirely rather than sanitizing
        if (!headers.ContainsKey("X-XSS-Protection"))
        {
            headers.Append("X-XSS-Protection", "1; mode=block");
        }

        // Referrer-Policy
        // Controls how much referrer information is sent with requests
        // no-referrer: Never send referrer information
        if (!headers.ContainsKey("Referrer-Policy"))
        {
            headers.Append("Referrer-Policy", "no-referrer");
        }

        // Content-Security-Policy (CSP)
        // Restricts resources (scripts, styles, images, etc.) that can be loaded
        // default-src 'self': Only allow resources from same origin
        // script-src 'self': Only allow scripts from same origin
        // style-src 'self' 'unsafe-inline': Allow same-origin styles and inline styles (for Swagger UI)
        // img-src 'self' data: https:: Allow same-origin images, data URIs, and HTTPS images
        // font-src 'self': Only allow fonts from same origin
        // connect-src 'self': Only allow AJAX/WebSocket connections to same origin
        // frame-ancestors 'none': Don't allow embedding in frames (equivalent to X-Frame-Options: DENY)
        // base-uri 'self': Restrict <base> tag URLs
        // form-action 'self': Restrict form submission targets
        if (!headers.ContainsKey("Content-Security-Policy"))
        {
            headers.Append("Content-Security-Policy",
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "font-src 'self'; " +
                "connect-src 'self'; " +
                "frame-ancestors 'none'; " +
                "base-uri 'self'; " +
                "form-action 'self'");
        }

        // Permissions-Policy (formerly Feature-Policy)
        // Controls which browser features and APIs can be used
        // Denies access to: geolocation, microphone, camera, payment, USB
        if (!headers.ContainsKey("Permissions-Policy"))
        {
            headers.Append("Permissions-Policy",
                "geolocation=(), " +
                "microphone=(), " +
                "camera=(), " +
                "payment=(), " +
                "usb=()");
        }

        // Remove server header to avoid revealing server information
        headers.Remove("Server");

        // Remove X-Powered-By header to avoid revealing technology stack
        headers.Remove("X-Powered-By");

        _logger.LogDebug("Security headers added to response");

        await _next(context);
    }
}

/// <summary>
/// Extension methods for registering SecurityHeadersMiddleware.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds security headers middleware to the application pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
