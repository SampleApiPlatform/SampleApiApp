using Microsoft.Extensions.Options;
using SampleApi.Options;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ServiceTokenOptions _options;

    public TokenValidationMiddleware(RequestDelegate next, IOptions<ServiceTokenOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/dapr"))
        {
            await _next(context);
            return;
        }
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            var token = context.Request.Headers["X-Service-Token"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(token) || !_options.ValidTokens.Contains(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or missing service token.");
                return;
            }
        }
        if (context.Request.Path.StartsWithSegments("/auth"))
        {
            var token = context.Request.Headers["X-Service-Token"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(token) || !_options.ValidTokens.Contains(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or missing service token.");
                return;
            }
        }
        await _next(context);
    }
}
