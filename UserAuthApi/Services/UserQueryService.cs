using Microsoft.AspNetCore.Identity;
using UserAuthApi.Data;

namespace UserAuthApi.Services;

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
}