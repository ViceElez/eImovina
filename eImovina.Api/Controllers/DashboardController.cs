using System.Security.Claims;
using eImovina.Api.Data;
using eImovina.Api.Security;
using eImovina.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly eImovinaDbContext _context;

    public DashboardController(eImovinaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var dashboard = new DashboardDto
        {
            TotalEquipmentCount = await _context.Equipment.CountAsync(),

            AssignedEquipmentCount = await _context.Equipment
                .CountAsync(item => item.Status!.Name == "Zaduženo"),

            InServiceEquipmentCount = await _context.Equipment
                .CountAsync(item => item.Status!.Name == "Na servisu"),

            MissingEquipmentCount = await _context.Equipment
                .CountAsync(item => item.Status!.Name == "Nedostaje"),

            OpenEquipmentRequestsCount = await _context.EquipmentRequests
                .CountAsync(item => item.Status!.Name != "Zatvoreno" && item.Status.Name != "Odbijeno"),

            InventoriesInProgressCount = await _context.Inventories
                .CountAsync(item => item.Status!.Name == "Otvorena" || item.Status.Name == "U tijeku"),

            TotalEquipmentValue = await _context.Equipment.SumAsync(item => (decimal?)item.Value) ?? 0,

            RecentChanges = await GetRecentChangesAsync()
        };

        var employeeIdClaim = User.FindFirstValue(AppClaimTypes.EmployeeId);
        if (employeeIdClaim is not null)
        {
            var employeeId = int.Parse(employeeIdClaim);

            dashboard.MyActiveAssignmentsCount = await _context.EquipmentAssignments
                .CountAsync(item => item.EmployeeId == employeeId && item.Status!.Name == "Aktivno");

            dashboard.MyOpenRequestsCount = await _context.EquipmentRequests
                .CountAsync(item => item.EmployeeId == employeeId &&
                                     item.Status!.Name != "Zatvoreno" && item.Status.Name != "Odbijeno");
        }

        return Ok(dashboard);
    }

    private async Task<List<RecentChangeDto>> GetRecentChangesAsync()
    {
        var recentAssignments = await _context.EquipmentAssignments
            .Include(item => item.Equipment)
            .Include(item => item.Employee)
            .Include(item => item.Status)
            .OrderByDescending(item => item.AssignedAt)
            .Take(5)
            .Select(item => new RecentChangeDto
            {
                OccurredAt = item.AssignedAt,
                Description = $"{item.Equipment!.Name} - {item.Status!.Name.ToLower()} ({item.Employee!.FirstName} {item.Employee.LastName})"
            })
            .ToListAsync();

        var recentWriteOffs = await _context.WriteOffRequests
            .Include(item => item.Equipment)
            .Include(item => item.Status)
            .Where(item => item.Status!.Name == "Provedeno" && item.DecidedAt != null)
            .OrderByDescending(item => item.DecidedAt)
            .Take(5)
            .Select(item => new RecentChangeDto
            {
                OccurredAt = item.DecidedAt!.Value,
                Description = $"{item.Equipment!.Name} - otpisano"
            })
            .ToListAsync();

        return recentAssignments
            .Concat(recentWriteOffs)
            .OrderByDescending(item => item.OccurredAt)
            .Take(5)
            .ToList();
    }
}
