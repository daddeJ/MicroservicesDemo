using Microsoft.EntityFrameworkCore;
using UserAuthApi.Data;

public class LoggingDbContext : DbContext
{
    public LoggingDbContext(DbContextOptions<LoggingDbContext> options) : base(options)
    {
    }
    
    public DbSet<Applicationlog> Applicationlogs { get; set; }
    public DbSet<UserActivityLog> UserActivityLogs { get; set; }
    public DbSet<SecurityAuditLog>  SecurityAuditLogs { get; set; }
}