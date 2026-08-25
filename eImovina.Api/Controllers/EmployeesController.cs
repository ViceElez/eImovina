using eImovina.Api.Data;
using eImovina.Api.Security;
using eImovina.Shared.DTOs.Users;
using eImovina.Shared.Models.Users;
using eImovina.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly eImovinaDbContext _context;

    public EmployeesController(eImovinaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Policy = ClassAuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<List<EmployeeDto>>> GetEmployees()
    {
        var employees = await _context.Employees
            .Include(employee => employee.Location)
            .OrderBy(employee => employee.LastName)
            .Select(employee => new EmployeeDto
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                LocationId = employee.LocationId,
                Location = employee.Location!.Name,
                IsActive = employee.IsActive
            })
            .ToListAsync();

        return Ok(employees);
    }

    [HttpGet("lookup")]
    [Authorize(Policy = ClassAuthorizationPolicies.ManageEquipment)]
    public async Task<ActionResult<List<LookUpDto>>> GetEmployeesLookup()
    {
        var employees = await _context.Employees
            .Where(employee => employee.IsActive)
            .OrderBy(employee => employee.LastName)
            .Select(employee => new LookUpDto { Id = employee.Id, Name = employee.FirstName + " " + employee.LastName })
            .ToListAsync();

        return Ok(employees);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = ClassAuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<EmployeeDto>> GetEmployeeById(int id)
    {
        var employee = await _context.Employees
            .Include(item => item.Location)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (employee is null)
            return NotFound();

        return Ok(new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            LocationId = employee.LocationId,
            Location = employee.Location!.Name,
            IsActive = employee.IsActive
        });
    }

    [HttpPost]
    [Authorize(Policy = ClassAuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(SaveEmployeeDto dto)
    {
        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            LocationId = dto.LocationId,
            IsActive = dto.IsActive
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, dto);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = ClassAuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> UpdateEmployee(int id, SaveEmployeeDto dto)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is null)
            return NotFound();

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.LocationId = dto.LocationId;
        employee.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
