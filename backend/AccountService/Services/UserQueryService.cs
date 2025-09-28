using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using AccountService.Models;
using AccountService.Helpers;

namespace AccountService.Services;

public class UserQueryService : IUserQueryService
{
    private readonly UserManager<IdentityUser> _userManager;

    public UserQueryService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<PageResultDto<UserDto>> GetUsersAsync(
        IQueryable<IdentityUser> query, 
        IEnumerable<string>? roles = null, 
        IEnumerable<int>? tiers = null, 
        int pageNumber = 1,
        int pageSize = 10)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Max(pageSize, 1);
        
        var filteredUsers = new List<UserDto>();

        foreach (var user in query)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var userClaims = await _userManager.GetClaimsAsync(user);

            bool matchesRoles = roles == null || !roles.Any() || userRoles.Any();
            bool matchesTiers = tiers == null || !tiers.Any() || userClaims.Any();
            
            if (matchesRoles && matchesTiers)
            {
                filteredUsers.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = string.Join(", ", userRoles),
                    Tier = string.Join(", ", userClaims.Select(c => c.Value))
                });
            }
        }
        
        var totalItems = filteredUsers.Count;
        var pageUsers = filteredUsers
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PageResultDto<UserDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = pageUsers
        };
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(string userId, UpdateUserDto model)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, "User not found");
        
        if (!string.IsNullOrEmpty(model.Role))
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            await _userManager.AddToRoleAsync(user, model.Role);
        }
        
        var tierClaim = (await _userManager.GetClaimsAsync(user))
            .FirstOrDefault(c => c.Type == "Tier");
        if (tierClaim != null)
            await _userManager.RemoveClaimAsync(user, tierClaim);
        
        if (!DataSeeder.RoleTierMap.TryGetValue(model.Role, out var expectedTier) 
            || !string.Equals(expectedTier.ToString(), model.Tier.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return (false, $"Tier '{model.Tier}' is not valid for role '{model.Role}'.");
        }

        await _userManager.AddClaimAsync(user, new Claim("Tier", model.Tier.ToString()));

        return (true, null);
    }

    public bool IsAllowedAccess(List<string> RolesAccess, List<string> roles, List<string> claimValues, int minRange, int maxRange,
        out string? errorMessage)
    {
        errorMessage = null;

        if (roles == null || !roles.Any())
        {
            errorMessage = "User has no assigned roles.";
            return false;
        }
        
        var invalidRoles = roles.Except(RolesAccess, StringComparer.OrdinalIgnoreCase).ToList();
        if (invalidRoles.Any())
        {
            errorMessage = $"Allowed roles: {string.Join(", ", invalidRoles)}";
            return false;
        }

        if (claimValues == null || !claimValues.Any())
        {
            errorMessage = "User has no assigned tiers.";
            return false;
        }
        
        var parsedTiers = new List<int>();
        foreach (var claimValue in claimValues)
        {
            if (int.TryParse(claimValue, out var parsedTier)) parsedTiers.Add(parsedTier);
            else
            {
                errorMessage = $"Invalid tier: {claimValue}";
                return false;
            }
        }

        if (parsedTiers.Any(t => t < minRange || t > maxRange))
        {
            errorMessage = $"Tier must be  between {minRange} and {maxRange}.";
            return false;
        }
        
        return true;
    }
}