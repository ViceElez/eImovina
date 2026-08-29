using System.Security.Claims;
using eImovina.Api.Data;
using eImovina.Shared.Models.Equipments;
using Microsoft.AspNetCore.Authorization;
using eImovina.Shared.DTOs.Equipments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Controllers;

[ApiController]
[Route("api/assignments")]
[Authorize]
public class EquipmentAssignmentsController : ControllerBase
{
    private readonly eImovinaDbContext _context;

    public EquipmentAssignmentsController(eImovinaDbContext context)
    {
        _context = context;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("assign")]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> AssignEquipment(AssignEquipmentDto dto)
    {
        var equipment = await _context.Equipment
            .Include(item => item.Status)
            .FirstOrDefaultAsync(item => item.Id == dto.EquipmentId);

        if (equipment is null)
            return NotFound("Oprema nije pronađena.");

        if (equipment.Status?.Name == "Otpisano")
            return BadRequest("Otpisana oprema se ne može zadužiti.");

        var hasActiveAssignment = await _context.EquipmentAssignments
            .Include(assignment => assignment.Status)
            .AnyAsync(assignment => assignment.EquipmentId == dto.EquipmentId && assignment.Status!.Name == "Aktivno");

        if (hasActiveAssignment)
            return BadRequest("Oprema je već zadužena.");

        var activeStatusId = await GetAssignmentStatusIdAsync("Aktivno");
        var assignedStatusId = await GetEquipmentStatusIdAsync("Zaduženo");

        _context.EquipmentAssignments.Add(new EquipmentAssignment
        {
            EquipmentId = dto.EquipmentId,
            EmployeeId = dto.EmployeeId,
            StatusId = activeStatusId,
            AssignedAt = DateTime.UtcNow,
            AssignedByUserId = CurrentUserId,
            Note = dto.Note
        });

        equipment.StatusId = assignedStatusId;
        equipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("return")]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> ReturnEquipment(ReturnEquipmentDto dto)
    {
        var assignment = await _context.EquipmentAssignments
            .Include(item => item.Status)
            .Include(item => item.Equipment)
            .FirstOrDefaultAsync(item => item.Id == dto.AssignmentId);

        if (assignment is null)
            return NotFound("Zaduženje nije pronađeno.");

        if (assignment.Status?.Name != "Aktivno")
            return BadRequest("Zaduženje nije aktivno.");

        var returnedStatusId = await GetAssignmentStatusIdAsync("Vraćeno");
        var inStockStatusId = await GetEquipmentStatusIdAsync("Na skladištu");

        assignment.StatusId = returnedStatusId;
        assignment.ReturnedAt = DateTime.UtcNow;
        assignment.ReturnedByUserId = CurrentUserId;
        if (!string.IsNullOrWhiteSpace(dto.Note))
            assignment.Note = dto.Note;

        assignment.Equipment!.StatusId = inStockStatusId;
        assignment.Equipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("transfer")]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> TransferEquipment(TransferEquipmentDto dto)
    {
        var activeAssignment = await _context.EquipmentAssignments
            .Include(item => item.Status)
            .FirstOrDefaultAsync(item => item.EquipmentId == dto.EquipmentId && item.Status!.Name == "Aktivno");

        if (activeAssignment is null)
            return BadRequest("Oprema trenutačno nije zadužena, koristite zaduživanje umjesto prijenosa.");

        var transferredStatusId = await GetAssignmentStatusIdAsync("Premješteno");
        var activeStatusId = await GetAssignmentStatusIdAsync("Aktivno");

        activeAssignment.StatusId = transferredStatusId;
        activeAssignment.ReturnedAt = DateTime.UtcNow;
        activeAssignment.ReturnedByUserId = CurrentUserId;

        _context.EquipmentAssignments.Add(new EquipmentAssignment
        {
            EquipmentId = dto.EquipmentId,
            EmployeeId = dto.NewEmployeeId,
            StatusId = activeStatusId,
            AssignedAt = DateTime.UtcNow,
            AssignedByUserId = CurrentUserId,
            Note = dto.Note
        });

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("change-location")]
    [Authorize(Roles = "Admin,InventoryManager")]
    public async Task<IActionResult> ChangeEquipmentLocation(ChangeEquipmentLocationDto dto)
    {
        var equipment = await _context.Equipment.FindAsync(dto.EquipmentId);
        if (equipment is null)
            return NotFound("Oprema nije pronađena.");

        equipment.CurrentLocationId = dto.NewLocationId;
        equipment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Employee,Admin,InventoryManager,LocationResponsible")]
    public async Task<ActionResult<List<EquipmentAssignmentDto>>> GetMyAssignments()
    {
        var employeeIdClaim = User.FindFirstValue("EmployeeId");
        if (employeeIdClaim is null)
            return BadRequest("Prijavljeni korisnik nema povezan Employee profil.");

        var employeeId = int.Parse(employeeIdClaim);

        var assignments = await _context.EquipmentAssignments
            .Include(assignment => assignment.Equipment)
            .Include(assignment => assignment.Status)
            .Include(assignment => assignment.AssignedByUser)
            .Where(assignment => assignment.EmployeeId == employeeId && assignment.Status!.Name == "Aktivno")
            .Select(assignment => new EquipmentAssignmentDto
            {
                Id = assignment.Id,
                EquipmentId = assignment.EquipmentId,
                Equipment = assignment.Equipment!.Name,
                InventoryNumber = assignment.Equipment.InventoryNumber,
                EmployeeId = assignment.EmployeeId,
                StatusId = assignment.StatusId,
                Status = assignment.Status!.Name,
                AssignedAt = assignment.AssignedAt,
                ReturnedAt = assignment.ReturnedAt,
                AssignedBy = assignment.AssignedByUser!.Username,
                Note = assignment.Note
            })
            .ToListAsync();

        return Ok(assignments);
    }
    private async Task<int> GetAssignmentStatusIdAsync(string name)
    {
        var status = await _context.AssignmentStatuses.FirstOrDefaultAsync(item => item.Name == name);
        return status?.Id ?? throw new InvalidOperationException($"AssignmentStatus '{name}' nije seed-an u bazi.");
    }

    private async Task<int> GetEquipmentStatusIdAsync(string name)
    {
        var status = await _context.EquipmentStatuses.FirstOrDefaultAsync(item => item.Name == name);
        return status?.Id ?? throw new InvalidOperationException($"EquipmentStatus '{name}' nije seed-an u bazi.");
    }
}
