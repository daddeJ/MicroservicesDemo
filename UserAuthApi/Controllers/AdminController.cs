using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserAuthApi.Data;
using UserAuthApi.Helpers;
using UserAuthApi.Services;

namespace UserAuthApi.Controllers;
/*
 *
 * Admin add Any User role and tier
 * 
 */
[ApiController]
[Route("api/admin")]
[Authorize(Policy = "SuperAdminOnly")]
public class AdminController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IUserQueryService _userQueryService;
    public AdminController(UserManager<IdentityUser> userManager, IUserQueryService userQueryService)
    {
        _userManager = userManager;
        _userQueryService = userQueryService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery(Name = "role")] string? role,
        [FromQuery(Name = "tier")] string? tier,
        [FromQuery(Name = "page")] int pageNumber = 1,
        [FromQuery(Name = "size")] int pageSize = 10)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;
        
        pageSize = pageSize > 100 ? 100 : pageSize;
        
        var allowedTiers = Enumerable.Range(0, 5).ToList();
        if (!QueryValidationHelper.TryValidateIntList(tier, allowedTiers, out var tierList, out var tierError))
            return BadRequest(new { message = tierError });

        var allowedRoles = DataSeeder.AdminRoleAccess;
        if (!QueryValidationHelper.TryValidateStringList(role, allowedRoles, out var roleList, out var roleError))
            return BadRequest(new { message = roleError });

        var queryableUsers = _userManager.Users.AsQueryable();

        var result = await _userQueryService.GetUsersAsync(queryableUsers, roleList, tierList, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();
        
        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        
        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            Roles = roles,
            Claims = claims.Select(c => new { c.Type, c.Value })
        });
    }

    [HttpPatch("users/{id}")]
    public async Task<IActionResult> UpdateUserRoleTier(string id, [FromBody] UpdateUserDto model)
    {
        var (success, error) = await _userQueryService.UpdateUserAsync(id, model);
        if (!success) return BadRequest(new { message = error });

        return Ok(new { message = "User role and tier updated successfully" });
    }


    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        await _userManager.UpdateAsync(user);
        
        return Ok(new { message = "User has been deleted successfully" });
    }
}