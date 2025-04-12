using System.Linq;
using Backend.Core.Entities;
using Backend.Core.Exceptions;
using Backend.Core.Interfaces;
using Backend.Infrastructure.Data;
using BCrypt.Net;

namespace Backend.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public ApplicationUser Register(string username, string email, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            throw new DomainException("Все поля (username, email, password) обязательны.");

        if (_context.Users.Any(u => u.Username == username))
            throw new AlreadyExistsException("Логин уже занят.");

        if (_context.Users.Any(u => u.Email == email))
            throw new AlreadyExistsException("Такой email уже зарегистрирован.");

        var newUser = new ApplicationUser
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _context.Users.Add(newUser);
        _context.SaveChanges();
        return newUser;
    }

    public ApplicationUser? Login(string usernameOrEmail, string password)
    {
        var user = _context.Users
            .FirstOrDefault(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
        if (user == null)
            return null;

        bool verified = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        return verified ? user : null;
    }
}