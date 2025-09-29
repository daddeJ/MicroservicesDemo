using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using UserAuthApi.Data;
using UserAuthApi.Helpers;
using UserAuthApi.Middlewares;
using UserAuthApi.Services;

Serilog.Debugging.SelfLog.Enable(msg =>
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Serilog ERROR: " + msg);
    Console.ResetColor();
});

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .Enrich.FromLogContext()
        .Enrich.WithCorrelationId()
        .Enrich.WithClientIp()
        .WriteTo.Console()
        .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
        .WriteTo.Async(a => a.MSSqlServer(
            connectionString: context.Configuration.GetConnectionString("LoggerConnection"),
            sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
            {
                TableName = "ApplicationLogs",
                AutoCreateSqlTable = true,
            },
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
        ))
        .WriteTo.Async(a => a.Console());
});

Log.Information("=== Serilog startup test entry ===");

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IAuditLoggerService, AuditLoggerService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<LoggingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LoggerConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization(options =>
{
    AuthorizationPolicies.AddTierPolicies(options);
});

builder.Services.AddControllers();
var app = builder.Build();

await DataSeeder.SeedRoles(app.Services);

app.MapHealthChecks("api/health");
app.UseMiddleware<EnhancedLoggingMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
