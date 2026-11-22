using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebAPI.Models;

namespace MyWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesDemoController : ControllerBase
{
    [Authorize(Roles = nameof(Roles.Admin))]
    [HttpGet("admin")]
    public IActionResult OnlyAdmins() => Ok("Admin access granted");

    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.Manager)}")]
    [HttpGet("management")]
    public IActionResult Management() => Ok("Admin access granted");

    [Authorize]
    [HttpGet("whoami")]
    public IActionResult WhoAmI()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }
}