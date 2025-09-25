using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserAuthApi.Data;
using UserAuthApi.Services;
using UserAuthApi.Helpers;

namespace UserAuthApi.Controllers;

[ApiController]
[Route("api/executive")]
[Authorize(Policy = "ExecutivesOnly")]
public class ExecutiveController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserQueryService  _userQueryService;
    
    public  ExecutiveController(UserManager<ApplicationUser> userManager, IUserQueryService userQueryService)
    {
        _userManager = userManager;
        _userQueryService = userQueryService;
    }
    
    // TODO: Read users list (tier 2-5, roles: HR, Manager, Leader, Regular)
    //  - GET <IP>/api/executive/users?tier=2,3,4,5&role=HR,Manager,Leader,Regular
    //  - Query:
    //      * tier <2, 3, 4, 5>
    //      * role <HR, Manager, Leader, Regular>
    //      * pageNumber min default <1>
    //      * pageSize min default <10>
    //  - Constraint: 
    //      * if tier < 1 && if tier > 6 return bad request
    //      * if role != HR, Manger, Leader, Regular return bad request

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery(Name = "role")] string? role,
        [FromQuery(Name = "tier")] string? tier,
        [FromQuery(Name = "page")] int pageNumber,
        [FromQuery(Name = "size")] int pageSize)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;
        
        pageSize = pageSize > 100 ? 100 : pageSize;

        var allowedTiers = Enumerable.Range(2, 5).ToList();
        if (!QueryValidationHelper.TryValidateIntList(tier, allowedTiers, out var tierList, out var tierError))
            return BadRequest(new { message = tierError });

        var allowedRoles = DataSeeder.ExecutiveRoleAccess;
        if (!QueryValidationHelper.TryValidateStringList(role, allowedRoles, out var roleList, out var roleError))
            return BadRequest(new { message = roleError });

        var queryableUsers = _userManager.Users.AsQueryable();

        var result = await _userQueryService.GetUsersAsync(queryableUsers, roleList, tierList, pageNumber, pageSize);
        return Ok(result);
    }
    
    // TODO: Read single user (tier 2-5, roles: HR, Manager, Leader, Regular)
    //  - GET <IP>/api/executive/users/{id}
    //  - Query:
    //      * id <UserId>
    //      * tier <2, 3, 4, 5>
    //      * role <HR, Manager, Leader, Regular>
    //  - Constraint: 
    //      * if tier < 1 && if tier > 6 return bad request
    //      * if role != HR, Manger, Leader, Regular return bad request
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();
        
        var userRoles = await _userManager.GetRolesAsync(user);
        var userClaims = await _userManager.GetClaimsAsync(user);
        var claimValues = userClaims.Select(c => c.Value).ToList();
        var allowedRoles = DataSeeder.ExecutiveRoleAccess;
        
        bool canView = _userQueryService.IsAllowedAccess(
            allowedRoles, userRoles.ToList(),
            claimValues,
            2,
            5,
            out var errorAccess);
        
        if (!canView) return BadRequest(new { message = errorAccess });
        
        return Ok(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = string.Join(", ", userRoles),
            Tier = string.Join(", ", userClaims.Select(c => c.Value))
        });
    }
    
    
    // TODO: Update user tier/role (tier 2-5, roles: HR, Manager, Leader, Regular)
    //  - PATCH <IP>/api/executive/users/{id}
    //  - Query:
    //      * id <UserId>
    //  - Constraint: 
    //      * if tier < 1 && if tier > 6 return bad request
    //      * if role != HR, Manger, Leader, Regular return bad request
    [HttpPatch("users/{id}")]
    public async Task<IActionResult> UpdateUserRoleTier(string id, [FromBody] UpdateUserDto model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();
        
        var userRoles = await _userManager.GetRolesAsync(user);
        var userClaims = await _userManager.GetClaimsAsync(user);
        var claimValues = userClaims.Select(c => c.Value).ToList();
        var allowedRoles = DataSeeder.ExecutiveRoleAccess;
        
        bool canDelete = _userQueryService.IsAllowedAccess(
            allowedRoles, userRoles.ToList(),
            claimValues,
            2,
            5,
            out var errorAccess);
        
        if (!canDelete) return BadRequest(new { message = errorAccess });
        
        var (success, error) = await _userQueryService.UpdateUserAsync(id, model);
        if (!success) return BadRequest(new { message = error });

        return Ok(new { message = "User role and tier updated successfully" });
    }
    
    // TODO: Delete user (tier 2-5, roles: HR, Manager, Leader, Regular)
    //  - DELETE <IP>/api/executive/users/{id}
    //  - Query:
    //      * id <UserId>
    //  - Constraint: 
    //      * if tier < 1 && if tier > 6 return bad request
    //      * if role != HR, Manger, Leader, Regular return bad request
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();
        
        var userRoles = await _userManager.GetRolesAsync(user);
        var userClaims = await _userManager.GetClaimsAsync(user);
        var claimValues = userClaims.Select(c => c.Value).ToList();
        var allowedRoles = DataSeeder.ExecutiveRoleAccess;
        
        bool canDelete = _userQueryService.IsAllowedAccess(
            allowedRoles, userRoles.ToList(),
            claimValues,
            2,
            5,
            out var error);

        if (!canDelete) return BadRequest(new { message = error });

        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        await _userManager.UpdateAsync(user);
        return Ok(new { message = "User has been deleted successfully" });
    }
}