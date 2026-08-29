using eImovina.Api.Data;
using eImovina.Api.Security;
using eImovina.Shared.DTOs.Auth;
using eImovina.Shared.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly eImovinaDbContext _context;
    private readonly JwtTokenService _tokenService;

    public AuthController(eImovinaDbContext context, JwtTokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var username = request.Username.Trim();

        var user = await _context.Users
            .Include(item => item.UserRoles)
                .ThenInclude(item => item.Role)
            .FirstOrDefaultAsync(item => item.Username == username);

        if (user is null || !user.IsActive)
            return Unauthorized("Pogrešna email adresa ili lozinka.");

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("Pogrešno email adresa ili lozinka.");

        var roles = user.UserRoles
            .Where(item => item.Role is not null)
            .Select(item => item.Role!.Name)
            .OrderBy(name => name)
            .ToList();

        return Ok(_tokenService.CreateLoginResponse(user, roles));
    }

    [HttpGet("me")]
    public ActionResult<LoggedUserDto> Me()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null)
            return Unauthorized();

        return Ok(new LoggedUserDto
        {
            UserId = int.Parse(userIdClaim),
            Username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? string.Empty,
            EmployeeId = User.FindFirst(AppClaimTypes.EmployeeId)?.Value is { } empId ? int.Parse(empId) : null,
            Roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList()
        });
    }
}