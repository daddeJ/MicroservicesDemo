using UserAuthApi.Data;

namespace UserAuthApi.Services;

public class AuditLoggerService : IAuditLoggerService
{
    private readonly ILogger<AuditLoggerService> _logger;
    private readonly LoggingDbContext _loggingDbContext;

    public AuditLoggerService(LoggingDbContext loggingDbContext, ILogger<AuditLoggerService> logger)
    {
        _logger = logger;
        _loggingDbContext = loggingDbContext;
    }
    
    public async Task LogUserActivityAsync(string userId, string activity, string ip)
    {
        _logger.LogInformation("UserActivity: {UserId} - {activity} (IP: {Ip})"
            , userId, activity, ip);

        _loggingDbContext.UserActivityLogs.Add(new UserActivityLog
        {
            UserId = userId,
            Activity = activity,
            IpAddress = ip,
            Timestamp = DateTime.UtcNow
        });
        
        await _loggingDbContext.SaveChangesAsync();
    }

    public async Task LogSecurityEventAsync(string @event, string details, string ip)
    {
        _logger.LogWarning("SecurityAudit: {Event} - {Details} (IP: {Ip})", @event, details, ip);
        
        _loggingDbContext.SecurityAuditLogs.Add(new SecurityAuditLog
        {
            Event = @event,
            Details = details,
            IpAddress = ip,
            Timestamp = DateTime.UtcNow
        });
        
        await _loggingDbContext.SaveChangesAsync();
    }
}