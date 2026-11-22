using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebAPI.Dtos;
using MyWebAPI.Helpers;
using MyWebAPI.Models;
using MyWebAPI.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;

namespace MyWebAPI.Controllers;

public class LoginModel
{
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly IUserService _userService;
	private readonly JwtTokenGenerator _jwtTokenGenerator;
	private readonly IPasswordHasher _passwordHasher;
	private readonly IMapper _mapper;

    public AuthController(IUserService userService, JwtTokenGenerator jwtTokenGenerator, IPasswordHasher passwordHasher, IMapper mapper)
	{
		_userService = userService;
		_jwtTokenGenerator = jwtTokenGenerator;
		_passwordHasher = passwordHasher;
		_mapper = mapper;
	}

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto dto)
    {
        var existingUser = await _userService.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            return BadRequest("Користувач уже існує.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            Password = _passwordHasher.HashPassword(dto.Password),
            Role = Roles.User
        };

        await _userService.CreateAsync(user);

        return Ok("Користувач зареєстрований.");
    }

    [HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginModel model)
	{
		var user = await _userService.GetByEmailAsync(model.Email);

		if (user == null || user.Password != _passwordHasher.HashPassword(model.Password))
			return Unauthorized("Неправильна пошта або пароль.");

		var token = _jwtTokenGenerator.Generate(user);
		var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

		user.RefreshToken = refreshToken;
		user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

		await _userService.UpdateAsync(user);

		return Ok(new
		{
			Token = token,
			RefreshToken = refreshToken,
			TokenExpiryTime = DateTime.UtcNow.AddMinutes(60),
			RefreshTokenExpiryTime = user.RefreshTokenExpiryTime
		});
	}

	[HttpPost("refresh")]
	public async Task<IActionResult> Refresh([FromHeader(Name = "Authorization")] string authHeader, [FromBody] string refreshToken)
	{
		if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
			return BadRequest("Missing or invalid Authorization header");

		var token = authHeader.Substring("Bearer ".Length);
		var principal = _jwtTokenGenerator.GetPrincipalFromExpiredToken(token);

		if (principal == null)
			return BadRequest("Invalid refresh token");

		var id = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
		if (id == null) return Unauthorized("Invalid token claims");

		var user = await _userService.GetAsync(id);
		if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
			return Unauthorized("Invalid refresh token");

		var newAccessToken = _jwtTokenGenerator.Generate(user);
		var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

		user.RefreshToken = newRefreshToken;
		user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

		await _userService.UpdateAsync(user);

		return Ok(new
		{
			token = newAccessToken,
			refreshToken = newRefreshToken,
			TokenExpiryTime = DateTime.UtcNow.AddMinutes(60),
			RefreshTokenExpiryTime = user.RefreshTokenExpiryTime
		});
	}

    //[HttpPost("register-admin")]
    //public async Task<IActionResult> RegisterAdmin([FromBody] CreateUserDto dto)
    //{
    //    var existingUser = await _userService.GetByEmailAsync(dto.Email);
    //    if (existingUser != null)
    //        return BadRequest("Користувач уже існує.");

    //    var admin = _mapper.Map<User>(dto);
    //    admin.Password = _passwordHasher.HashPassword(dto.Password);
    //    admin.Role = Roles.Admin;

    //    await _userService.CreateAsync(admin);

    //    return Ok("Адміністратора зареєстровано.");
    //}
}