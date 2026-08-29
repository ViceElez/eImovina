using System.Security.Claims;
using eImovina.Api.Data;
using eImovina.Shared.DTOs.WriteOffRequests;
using eImovina.Shared.Models.WriteOffRequests;
using eImovina.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Controllers;

[ApiController]
[Route("api/writeoffrequests")]
[Authorize]
public class WriteOffRequestsController : ControllerBase
{
    private readonly eImovinaDbContext _context;

    public WriteOffRequestsController(eImovinaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Policy = ClassAuthorizationPolicies.ManageEquipment)]
    public async Task<ActionResult<List<WriteOffRequestDto>>> GetWriteOffRequests([FromQuery] int? statusId)
    {
        var query = _context.WriteOffRequests
            .Include(request => request.Equipment)
            .Include(request => request.Status)
            .Include(request => request.RequestedByUser)
            .Include(request => request.DecisionByUser)
            .AsQueryable();

        if (statusId.HasValue)
            query = query.Where(request => request.StatusId == statusId.Value);

        var requests = await query
            .OrderByDescending(request => request.RequestedAt)
            .Select(request => ToDto(request))
            .ToListAsync();

        return Ok(requests);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = ClassAuthorizationPolicies.ManageEquipment)]
    public async Task<ActionResult<WriteOffRequestDto>> GetWriteOffRequestById(int id)
    {
        var request = await _context.WriteOffRequests
            .Include(item => item.Equipment)
            .Include(item => item.Status)
            .Include(item => item.RequestedByUser)
            .Include(item => item.DecisionByUser)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (request is null)
            return NotFound();

        return Ok(ToDto(request));
    }

    [HttpPost]
    [Authorize(Policy = ClassAuthorizationPolicies.ManageEquipment)]
    public async Task<IActionResult> CreateWriteOffRequest(SaveWriteOffRequestDto dto)
    {
        var equipment = await _context.Equipment
            .Include(item => item.Status)
            .FirstOrDefaultAsync(item => item.Id == dto.EquipmentId);

        if (equipment is null)
            return NotFound("Oprema nije pronađena.");

        if (equipment.Status?.Name == "Otpisano")
            return BadRequest("Oprema je već otpisana.");

        var hasOpenRequest = await _context.WriteOffRequests
            .Include(item => item.Status)
            .AnyAsync(item => item.EquipmentId == dto.EquipmentId &&
                               item.Status!.Name != "Odbijeno" && item.Status.Name != "Provedeno");

        if (hasOpenRequest)
            return BadRequest("Za ovu opremu već postoji otvoren zahtjev za otpis.");

        var receivedStatusId = await GetWriteOffStatusIdAsync("Zaprimljeno");

        var request = new WriteOffRequest
        {
            EquipmentId = dto.EquipmentId,
            RequestedByUserId = CurrentUserId,
            StatusId = receivedStatusId,
            Reason = dto.Reason,
            RequestedAt = DateTime.UtcNow
        };

        _context.WriteOffRequests.Add(request);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWriteOffRequestById), new { id = request.Id }, new { id = request.Id });
    }

    [HttpPost("{id}/decide")]
    [Authorize(Policy = ClassAuthorizationPolicies.ManageEquipment)]
    public async Task<IActionResult> DecideWriteOffRequest(int id, DecideWriteOffRequestDto dto)
    {
        var request = await _context.WriteOffRequests
            .Include(item => item.Status)
            .Include(item => item.Equipment)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (request is null)
            return NotFound();

        if (request.Status?.Name is "Odbijeno" or "Provedeno")
            return BadRequest("Zahtjev je već zatvoren.");

        request.StatusId = dto.StatusId;
        request.DecisionNote = dto.DecisionNote;
        request.DecisionByUserId = CurrentUserId;
        request.DecidedAt = DateTime.UtcNow;

        var newStatus = await _context.WriteOffRequestStatuses.FindAsync(dto.StatusId);

        if (newStatus?.Name == "Provedeno")
        {
            var writtenOffStatusId = await GetEquipmentStatusIdAsync("Otpisano");
            request.Equipment!.StatusId = writtenOffStatusId;
            request.Equipment.UpdatedAt = DateTime.UtcNow;
        }

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

    private async Task<int> GetWriteOffStatusIdAsync(string name)
    {
        var status = await _context.WriteOffRequestStatuses.FirstOrDefaultAsync(item => item.Name == name);
        return status?.Id ?? throw new InvalidOperationException($"WriteOffRequestStatus '{name}' nije seed-an u bazi.");
    }

    private async Task<int> GetEquipmentStatusIdAsync(string name)
    {
        var status = await _context.EquipmentStatuses.FirstOrDefaultAsync(item => item.Name == name);
        return status?.Id ?? throw new InvalidOperationException($"EquipmentStatus '{name}' nije seed-an u bazi.");
    }

    private static WriteOffRequestDto ToDto(WriteOffRequest request) => new()
    {
        Id = request.Id,
        EquipmentId = request.EquipmentId,
        Equipment = request.Equipment?.Name ?? string.Empty,
        InventoryNumber = request.Equipment?.InventoryNumber ?? string.Empty,
        RequestedBy = request.RequestedByUser?.Username ?? string.Empty,
        StatusId = request.StatusId,
        Status = request.Status?.Name ?? string.Empty,
        Reason = request.Reason,
        RequestedAt = request.RequestedAt,
        DecisionBy = request.DecisionByUser?.Username,
        DecisionNote = request.DecisionNote,
        DecidedAt = request.DecidedAt
    };
}
