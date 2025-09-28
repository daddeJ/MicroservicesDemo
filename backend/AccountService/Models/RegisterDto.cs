using System.ComponentModel.DataAnnotations;

namespace AccountService.Models;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [MinLength(6)]
    public string UserName { get; set; }
    
    [Required]
    [MinLength(6, ErrorMessage = "Password must have at least 6 characters")]
    public string Password { get; set; }
    
    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
    
    public string Role { get; set; }
    
    [Range(0, 5, ErrorMessage = "Tier must be between 0 (SuperAdmin) and 5 (Regular).")]
    public int Tier { get; set; } = 5;
}