using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserAuthApi.Data;
using UserAuthApi.Helpers;
using UserAuthApi.Models;
using UserAuthApi.Services;

namespace UserAuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AccountController(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
        };
        
        var result = await _userManager.CreateAsync(user, model.Password);
        
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        if (string.IsNullOrEmpty(model.Role))
        {
            return BadRequest(new { message = "Account must have a role" });
        }
        
        await _userManager.AddToRoleAsync(user, model.Role);
        
        if (!DataSeeder.RoleTierMap.TryGetValue(model.Role, out var expectedTier) 
            || !string.Equals(expectedTier.ToString(), model.Tier.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = $"Tier '{model.Tier}' is not valid for role '{model.Role}'." });
        }

        await _userManager.AddClaimAsync(user, new Claim("Tier", model.Tier.ToString()));
        
        var token = _jwtTokenService.GenerateJwtToken(user);
        
        return Ok(new {Message = "Account created successfully!", Token = token});
    }
    
    [Authorize(Policy = "RegularAndAbove")]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);

        var claims = await _userManager.GetClaimsAsync(user);

        return Ok(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = string.Join(", ", roles),
            Tier = string.Join(", ", claims.Select(c => c.Value))
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
            return Unauthorized(new { message = "Invalid username or password." });

        var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!passwordValid)
            return Unauthorized(new { message = "Invalid username or password." });

        var token = await _jwtTokenService.GenerateJwtToken(user);
        
        return Ok(new
        {
            Message = "Account logged in!", 
            Token = token
        });
    }
}