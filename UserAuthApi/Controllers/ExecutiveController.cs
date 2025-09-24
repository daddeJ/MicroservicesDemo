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
    //  - Constraint 
    //      * if tier < 1 return bad request
    //      * if role != HR, Manger, Leader, Regular return bad request
    
    // TODO: Update user tier/role (tier 2-5, roles: HR, Manager, Leader, Regular)
    //  - PATCH <IP>/api/executive/users/{id}
    //  - Constraint 
    //      * if tier < 1 return bad request
    //      * if role != HR, Manger, Leader, Regular return bad request
    
    // TODO: Delete user (tier 2-5, roles: HR, Manager, Leader, Regular)
    //  - DELETE <IP>/api/executive/users/{id}
    //  - Constraint 
    //      * if tier < 1 return bad request
    //      * if role != HR, Manger, Leader, Regular return bad request
}