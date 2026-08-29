using System.Security.Claims;
using eImovina.Api.Data;
using eImovina.Shared.Models.Equipments;
using eImovina.Api.Security;
using eImovina.Shared.DTOs.EquipmentRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Controllers;

[ApiController]
[Route("api/equipmentrequests")]
[Authorize]
public class EquipmentRequestsController : ControllerBase
{
    private readonly eImovinaDbContext _context;

    public EquipmentRequestsController(eImovinaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Policy = ClassAuthorizationPolicies.ManageEquipment)]
    public async Task<ActionResult<List<EquipmentRequestDto>>> GetEquipmentRequests([FromQuery] int? statusId, [FromQuery] int? categoryId)
    {
        var query = _context.EquipmentRequests
            .Include(request => request.Employee)
            .Include(request => request.Category)
            .Include(request => request.Status)
            .Include(request => request.ResolvedByUser)
            .AsQueryable();

        if (statusId.HasValue)
            query = query.Where(request => request.StatusId == statusId.Value);

        if (categoryId.HasValue)
            query = query.Where(request => request.CategoryId == categoryId.Value);

        var requests = await query
            .OrderByDescending(request => request.RequestedAt)
            .Select(request => ToDto(request))
            .ToListAsync();

        return Ok(requests);
    }

    [HttpGet("mine")]
    [Authorize(Policy = ClassAuthorizationPolicies.EmployeeAccess)]
    public async Task<ActionResult<List<EquipmentRequestDto>>> GetMyRequests()
    {
        var employeeIdClaim = User.FindFirstValue(AppClaimTypes.EmployeeId);
        if (employeeIdClaim is null)
            return BadRequest("Prijavljeni korisnik nema povezan Employee profil.");

        var employeeId = int.Parse(employeeIdClaim);

        var requests = await _context.EquipmentRequests
            .Include(request => request.Employee)
            .Include(request => request.Category)
            .Include(request => request.Status)
            .Include(request => request.ResolvedByUser)
            .Where(request => request.EmployeeId == employeeId)
            .OrderByDescending(request => request.RequestedAt)
            .Select(request => ToDto(request))
            .ToListAsync();

        return Ok(requests);
    }

    [HttpPost("mine")]
    [Authorize(Policy = ClassAuthorizationPolicies.EmployeeAccess)]
    public async Task<IActionResult> CreateMyRequest(SaveEquipmentRequestDto dto)
    {
        var employeeIdClaim = User.FindFirstValue(AppClaimTypes.EmployeeId);
        if (employeeIdClaim is null)
            return BadRequest("Prijavljeni korisnik nema povezan Employee profil.");

        var receivedStatusId = await GetRequestStatusIdAsync("Zaprimljeno");

        var request = new EquipmentRequest
        {
            EmployeeId = int.Parse(employeeIdClaim),
            CategoryId = dto.CategoryId,
            StatusId = receivedStatusId,
            Description = dto.Description,
            RequestedAt = DateTime.UtcNow
        };

        _context.EquipmentRequests.Add(request);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/resolve")]
    [Authorize(Policy = ClassAuthorizationPolicies.ManageEquipment)]
    public async Task<IActionResult> ResolveRequest(int id, ResolveEquipmentRequestDto dto)
    {
        var request = await _context.EquipmentRequests.FindAsync(id);
        if (request is null)
            return NotFound();

        request.StatusId = dto.StatusId;
        request.ResolutionNote = dto.ResolutionNote;
        request.ResolvedAt = DateTime.UtcNow;
        request.ResolvedByUserId = CurrentUserId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    private int CurrentUserId
    {
        get
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim is not null ? int.Parse(claim) : 1; // TODO: makni fallback kad auth radi
        }
    }

    private async Task<int> GetRequestStatusIdAsync(string name)
    {
        var status = await _context.RequestStatuses.FirstOrDefaultAsync(item => item.Name == name);
        return status?.Id ?? throw new InvalidOperationException($"RequestStatus '{name}' nije seed-an u bazi.");
    }

    private static EquipmentRequestDto ToDto(EquipmentRequest request) => new()
    {
        Id = request.Id,
        EmployeeId = request.EmployeeId,
        Employee = request.Employee != null ? request.Employee.FirstName + " " + request.Employee.LastName : string.Empty,
        CategoryId = request.CategoryId,
        Category = request.Category?.Name ?? string.Empty,
        StatusId = request.StatusId,
        Status = request.Status?.Name ?? string.Empty,
        Description = request.Description,
        RequestedAt = request.RequestedAt,
        ResolvedAt = request.ResolvedAt,
        ResolvedBy = request.ResolvedByUser?.Username,
        ResolutionNote = request.ResolutionNote
    };
}
