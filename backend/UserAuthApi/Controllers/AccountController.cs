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
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        _logger.LogInformation("Register attempt for username={Username}", model.UserName);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid registration model for username={Username}", model.UserName);
            return BadRequest(ModelState);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
        };
        
        var result = await _userManager.CreateAsync(user, model.Password);
        
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to create user {Username}: {Errors}", model.UserName, 
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(result.Errors);
        }

        if (string.IsNullOrEmpty(model.Role))
        {
            _logger.LogWarning("Registration failed: role not provided for username={Username}", model.UserName);
            return BadRequest(new { message = "Account must have a role" });
        }
        
        await _userManager.AddToRoleAsync(user, model.Role);

        if (!DataSeeder.RoleTierMap.TryGetValue(model.Role, out var expectedTier) 
            || !string.Equals(expectedTier.ToString(), model.Tier.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Registration failed: tier '{Tier}' invalid for role '{Role}' for username={Username}", 
                model.Tier, model.Role, model.UserName);
            return BadRequest(new { message = $"Tier '{model.Tier}' is not valid for role '{model.Role}'." });
        }

        await _userManager.AddClaimAsync(user, new Claim("Tier", model.Tier.ToString()));
        
        var token = _jwtTokenService.GenerateJwtToken(user);
        _logger.LogInformation("User {Username} registered successfully", model.UserName);
        
        return Ok(new { Message = "Account created successfully!", Token = token });
    }
    
    [Authorize(Policy = "RegularAndAbove")]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            _logger.LogWarning("Unauthorized 'Me' request.");
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        _logger.LogInformation("User {Username} accessed 'Me' endpoint", user.UserName);

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
        _logger.LogInformation("Login attempt for username={Username}", model.Username);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid login model for username={Username}", model.Username);
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
        {
            _logger.LogWarning("Login failed: username {Username} not found", model.Username);
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!passwordValid)
        {
            _logger.LogWarning("Login failed: invalid password for username={Username}", model.Username);
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var token = await _jwtTokenService.GenerateJwtToken(user);
        _logger.LogInformation("User {Username} logged in successfully", model.Username);
        
        return Ok(new
        {
            Message = "Account logged in!", 
            Token = token
        });
    }
}
