namespace UserAuthApi.Middlewares;

public class HoneypotMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HoneypotMiddleware> _logger;

    public HoneypotMiddleware(RequestDelegate next, ILogger<HoneypotMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext.Request.Method == HttpMethods.Post)
        {
            if (httpContext.Request.HasFormContentType)
            {
                var form = await httpContext.Request.ReadFormAsync();
                if (!string.IsNullOrEmpty(form["honeypot"]))
                {
                    _logger.LogWarning("Honeypot triggered from IP {IP} on {Path}",
                        httpContext.Connection.RemoteIpAddress, httpContext.Request.Path);
                    
                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await httpContext.Response.WriteAsync("Forbidden");
                    return;
                }
            }
        }
        await _next(httpContext);
    }
}