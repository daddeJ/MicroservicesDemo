using System.ComponentModel.DataAnnotations;

namespace UserAuthApi.Models;

public class UpdateUserDto
{
    [Required]
    public string Role { get; set; }
    
    [Range(0, 5)]
    public int? Tier { get; set; }
}