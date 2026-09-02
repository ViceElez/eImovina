using eImovina.Api.Data;
using eImovina.Api.Security;
using eImovina.Shared.DTOs.Users;
using eImovina.Shared.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ClassAuthorizationPolicies.AdminOnly)]
public class UsersController : ControllerBase
{
    private readonly eImovinaDbContext _context;

    public UsersController(eImovinaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .Include(user => user.Employee)
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .OrderBy(user => user.Username)
            .Select(user => ToDto(user))
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        var user = await _context.Users
            .Include(item => item.Employee)
            .Include(item => item.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user is null)
            return NotFound();

        return Ok(ToDto(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(SaveUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username))
            return BadRequest("Korisničko ime je obavezno.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Lozinka je obavezna za novi račun.");

        var usernameTaken = await _context.Users.AnyAsync(item => item.Username == dto.Username);
        if (usernameTaken)
            return BadRequest("Korisničko ime je već zauzeto.");

        var user = new User
        {
            Username = dto.Username.Trim(),
            EmployeeId = dto.EmployeeId,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        foreach (var roleId in dto.RoleIds.Distinct())
            _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new { id = user.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, SaveUserDto dto)
    {
        var user = await _context.Users
            .Include(item => item.UserRoles)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user is null)
            return NotFound();

        var usernameTaken = await _context.Users.AnyAsync(item => item.Username == dto.Username && item.Id != id);
        if (usernameTaken)
            return BadRequest("Korisničko ime je već zauzeto.");

        user.Username = dto.Username.Trim();
        user.EmployeeId = dto.EmployeeId;
        user.IsActive = dto.IsActive;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, dto.Password);
        }

        _context.UserRoles.RemoveRange(user.UserRoles);
        foreach (var roleId in dto.RoleIds.Distinct())
            _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}/active")]
    public async Task<IActionResult> SetUserActive(int id, SetUserActiveDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
            return NotFound();

        user.IsActive = dto.IsActive;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        IsActive = user.IsActive,
        EmployeeId = user.EmployeeId,
        Employee = user.Employee != null ? user.Employee.FirstName + " " + user.Employee.LastName : null,
        Roles = user.UserRoles.Where(userRole => userRole.Role != null)
            .Select(userRole => userRole.Role!.Name)
            .OrderBy(name => name)
            .ToList(),
        CreatedAt = user.CreatedAt
    };
}
