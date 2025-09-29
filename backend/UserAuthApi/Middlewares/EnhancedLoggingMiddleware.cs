namespace UserAuthApi.Middlewares;

public class EnhancedLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EnhancedLoggingMiddleware> _logger;

    public EnhancedLoggingMiddleware(RequestDelegate next, ILogger<EnhancedLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();
        context.Items["correlationId"] = correlationId;
        
        var watch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await _next(context);
            watch.Stop();

            _logger.LogInformation(
                "Request {Method} {Path} completed in {Elapsed}ms with CorrelationId {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                watch.ElapsedMilliseconds,
                correlationId);
        }
        catch (Exception ex)
        {
            watch.Stop();
            _logger.LogInformation(
                "Request {Method} {Path} completed in {Elapsed}ms with CorrelationId {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                watch.ElapsedMilliseconds,
                correlationId);
            throw;
        }
    }
}