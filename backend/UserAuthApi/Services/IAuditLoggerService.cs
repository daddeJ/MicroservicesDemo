namespace UserAuthApi.Services;

public interface IAuditLoggerService
{
    Task LogUserActivityAsync(string userId, string activity, string ip);
    Task LogSecurityEventAsync(string @event, string details, string ip);
}