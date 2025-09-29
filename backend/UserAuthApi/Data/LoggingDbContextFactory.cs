using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserAuthApi.Data;

public class LoggingDbContextFactory : IDesignTimeDbContextFactory<LoggingDbContext>
{
    public LoggingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LoggingDbContext>();

        // Read the connection string from appsettings.json
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("LoggerConnection");
        optionsBuilder.UseSqlServer(connectionString);

        return new LoggingDbContext(optionsBuilder.Options);
    }
}