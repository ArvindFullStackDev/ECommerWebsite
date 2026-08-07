using Microsoft.AspNetCore.Identity;

namespace Identity.Interfaces;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(IdentityUser user);
}
