namespace UserAuthApi.Models;

public class UserDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Tier { get; set; }
}