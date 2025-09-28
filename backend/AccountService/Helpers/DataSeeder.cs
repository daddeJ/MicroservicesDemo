using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace AccountService.Helpers;

public static class DataSeeder
{
    public static readonly Dictionary<string, int> RoleTierMap = new Dictionary<string, int>()
    {
        { "Admin", 0 },
        { "Executive", 1 },
        { "HR", 2 },
        { "Manager", 3 },
        { "Leader", 4 },
        { "User", 5 }
    };
    
    public static readonly List<string> AdminRoleAccess = new List<string>()
    {
        "Admin",
        "Executive",
        "HR", 
        "Manager", 
        "Leader", 
        "Regular"
    };

    public static readonly List<string> ExecutiveRoleAccess = new List<string>()
    {
        "HR", 
        "Manager", 
        "Leader", 
        "Regular"
    };
    
    public static readonly List<string> ManagerRoleAccess = new List<string>()
    {
        "Leader", 
        "Regular"
    };

    public static readonly List<Claim> TierList = new List<Claim>()
    {
        new Claim("Tier", "0"),
        new Claim("Tier", "1"),
        new Claim("Tier", "2"),
        new Claim("Tier", "3"),
        new Claim("Tier", "4"),
        new Claim("Tier", "5")
    };
    
    public static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles =
        {
            "Admin",
            "Executive",
            "HR",
            "Manager",
            "Leader",
            "User"
        };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
    
}