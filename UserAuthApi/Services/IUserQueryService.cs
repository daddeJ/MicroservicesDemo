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
}