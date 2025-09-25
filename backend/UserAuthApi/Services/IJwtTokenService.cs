using UserAuthApi.Data;

namespace UserAuthApi.Services;

public interface IJwtTokenService
{
    Task<string> GenerateJwtToken(ApplicationUser user);
}