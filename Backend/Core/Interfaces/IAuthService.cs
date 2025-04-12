using Backend.Core.Entities;

namespace Backend.Core.Interfaces;

public interface IAuthService
{
    ApplicationUser Register(string username, string email, string password);
    ApplicationUser? Login(string usernameOrEmail, string password);
}