using Microsoft.AspNetCore.Identity;
using UserAuthApi.Data;

namespace UserAuthApi.Helpers;

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

    public static readonly List<string> ExecutiveRoleAccess = new List<string>()
    {
        "HR", 
        "Manager", 
        "Leader", 
        "Regular"
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