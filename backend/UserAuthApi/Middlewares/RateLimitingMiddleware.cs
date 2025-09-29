using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace UserAuthApi.Middlewares;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _memoryCache;
    private readonly int _maxRequest;
    private readonly TimeSpan _period;

    public RateLimitingMiddleware(RequestDelegate next,
        IMemoryCache memoryCache,
        int maxRequest, 
        TimeSpan period)
    {
        _next = next;
        _memoryCache = memoryCache;
        _maxRequest = maxRequest;
        _period = period;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var key = $"{context.Connection.RemoteIpAddress}:{context.Request.Path}";
        var count = _memoryCache.GetOrCreate<int>(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _period;
            return 0;
        });

        if (count >= _maxRequest)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            await context.Response.WriteAsync("Rate limit exceeded. Try");
            return;
        }

        _memoryCache.Set(key, count + 1);
        await _next(context);
    }
}