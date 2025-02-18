namespace AuthService.API.Middleware;

public class HybridAuthMiddleware
{
    private readonly RequestDelegate _next;

    public HybridAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            // Nếu có Authorization Header -> Dùng JWT
            context.Items["AuthScheme"] = "Bearer";
        }
        else if (context.Request.Cookies.ContainsKey(".AspNetCore.Cookies"))
        {
            // Nếu có Cookie -> Dùng Cookie Auth
            context.Items["AuthScheme"] = "Cookies";
        }

        await _next(context);
    }
}