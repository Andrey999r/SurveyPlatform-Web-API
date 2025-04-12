using Backend.Core.Entities;

namespace Backend.Core.Interfaces;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user);
}