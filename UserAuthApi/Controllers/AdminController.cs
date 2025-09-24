using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserAuthApi.Data;
using UserAuthApi.Helpers;

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
    
    public AdminController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery(Name = "role")] string? role,
        [FromQuery(Name = "tier")] int? tier,
        [FromQuery(Name = "pageNumber")] int pageNumber = 1,
        [FromQuery(Name = "pageSize")] int pageSize = 10)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;
        
        var users = _userManager.Users.ToList();
        var filteredUsers = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            
            bool matchesRoles = string.IsNullOrEmpty(role) || roles.Contains(role);
            bool matchesTier = string.IsNullOrEmpty(tier.ToString()) || claims.Any(c => c.Value == tier.Value.ToString());

            if (matchesRoles && matchesTier)
            {
                filteredUsers.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = string.Join(", ", roles),
                    Tier = string.Join(", ", claims.Select(c => c.Value))
                });
            }
            
            
        }
        var totalItems = filteredUsers.Count;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        
        var pageUsers = filteredUsers
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new PageResultDto<UserDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = pageUsers,
        };

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
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

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
            return BadRequest(new { message = $"Tier '{model.Tier}' is not valid for role '{model.Role}'." });
        }

        await _userManager.AddClaimAsync(user, new Claim("Tier", model.Tier.ToString()));

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