using Backend.Core.DTO.Account;
using Backend.Core.Exceptions;
using Backend.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;

    public AccountController(IAuthService authService, IJwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto dto)
    {
        try
        {
            var user = _authService.Register(dto.Username, dto.Email, dto.Password);
            return Ok(new { user.Id, user.Email });
        }
        catch (AlreadyExistsException ex)
        {
            return Conflict(ex.Message);
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        var user = _authService.Login(dto.UsernameOrEmail, dto.Password);
        if (user == null)
            return Unauthorized("Неверный логин или пароль.");

        var token = _jwtService.GenerateToken(user);
        // Возвращаем клиенту JWT-токен
        return Ok(new { Token = token });
    }
}
