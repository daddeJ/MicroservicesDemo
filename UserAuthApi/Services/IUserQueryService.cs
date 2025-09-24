using Microsoft.AspNetCore.Identity;
using UserAuthApi.Data;

namespace UserAuthApi.Services;

public interface IUserQueryService
{
    Task<PageResultDto<UserDto>> GetUsersAsync(
        IQueryable<IdentityUser> query,
        IEnumerable<string>? roles = null,
        IEnumerable<int>? tiers = null,
        int pageNumber = 1,
        int pageSize = 10);
    
    Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(
        string userId, UpdateUserDto user);

    bool IsAllowedAccess(
        List<string> RolesAccess,
        List<string> roles, 
        List<string> claimValues,
        int minRange,
        int maxRange,
        out string? errorMessage);
}