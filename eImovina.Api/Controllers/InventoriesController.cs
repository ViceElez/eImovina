using System.Security.Claims;
using eImovina.Api.Data;
using eImovina.Api.Security;
using eImovina.Shared.DTOs.Inventories;
using eImovina.Shared.Models.Inventories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class InventoriesController : ControllerBase
{
    private readonly eImovinaDbContext _context;

    public InventoriesController(eImovinaDbContext context)
    {
        _context = context;
    }

    private int? GetLocationRestriction()
    {
        if (User.IsInRole("LocationResponsible") && !User.IsInRole("Admin") && !User.IsInRole("InventoryManager"))
        {
            var claim = User.FindFirst(AppClaimTypes.LocationId)?.Value;
            return claim is not null ? int.Parse(claim) : null;
        }
        return null;
    }

    private int CurrentUserId
    {
        get
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim is not null ? int.Parse(claim) : 1; // TODO: makni fallback kad auth radi
        }
    }

    [HttpGet]
    //[Authorize(Policy = ClassAuthorizationPolicies.ViewEquipment)]
    public async Task<ActionResult<List<InventoryDto>>> GetInventories([FromQuery] int? statusId)
    {
        var query = _context.Inventories
            .Include(inventory => inventory.Location)
            .Include(inventory => inventory.ResponsibleEmployee)
            .Include(inventory => inventory.Status)
            .Include(inventory => inventory.Items)
            .AsQueryable();

        var locationRestriction = GetLocationRestriction();
        if (locationRestriction.HasValue)
            query = query.Where(inventory => inventory.LocationId == locationRestriction.Value);

        if (statusId.HasValue)
            query = query.Where(inventory => inventory.StatusId == statusId.Value);

        var inventories = await query
            .OrderByDescending(inventory => inventory.Id)
            .Select(inventory => ToDto(inventory))
            .ToListAsync();

        return Ok(inventories);
    }

    [HttpGet("{id}")]
    //[Authorize(Policy = ClassAuthorizationPolicies.ViewEquipment)]
    public async Task<ActionResult<InventoryDto>> GetInventoryById(int id)
    {
        var inventory = await _context.Inventories
            .Include(item => item.Location)
            .Include(item => item.ResponsibleEmployee)
            .Include(item => item.Status)
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (inventory is null)
            return NotFound();

        return Ok(ToDto(inventory));
    }

    [HttpPost]
    //[Authorize(Policy = ClassAuthorizationPolicies.ViewEquipment)]
    public async Task<ActionResult<InventoryDto>> CreateInventory(SaveInventoryDto dto)
    {
        var locationRestriction = GetLocationRestriction();
        if (locationRestriction.HasValue && dto.LocationId != locationRestriction.Value)
            return Forbid();

        var draftStatusId = await GetInventoryStatusIdAsync("Nacrt");

        var inventory = new Inventory
        {
            LocationId = dto.LocationId,
            ResponsibleEmployeeId = dto.ResponsibleEmployeeId,
            StatusId = draftStatusId,
            Note = dto.Note,
            CreatedByUserId = CurrentUserId
        };

        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetInventoryById), new { id = inventory.Id }, new { id = inventory.Id });
    }

    [HttpPost("{id}/open")]
    //[Authorize(Policy = ClassAuthorizationPolicies.ViewEquipment)]
    public async Task<IActionResult> OpenInventory(int id)
    {
        var inventory = await _context.Inventories
            .Include(item => item.Status)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (inventory is null)
            return NotFound();

        if (!CanManage(inventory))
            return Forbid();

        if (inventory.Status?.Name != "Nacrt")
            return BadRequest("Inventura mora biti u statusu 'Nacrt' da bi se otvorila.");

        var equipmentAtLocation = await _context.Equipment
            .Include(item => item.Status)
            .Where(item => item.CurrentLocationId == inventory.LocationId && item.Status!.Name != "Otpisano")
            .ToListAsync();

        foreach (var equipment in equipmentAtLocation)
        {
            _context.InventoryItems.Add(new InventoryItem
            {
                InventoryId = inventory.Id,
                EquipmentId = equipment.Id,
                ExpectedLocationId = equipment.CurrentLocationId,
                ExpectedInventoryNumber = equipment.InventoryNumber
            });
        }

        inventory.StatusId = await GetInventoryStatusIdAsync("Otvorena");
        inventory.OpenedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/complete")]
    //[Authorize(Policy = ClassAuthorizationPolicies.ViewEquipment)]
    public async Task<IActionResult> CompleteInventory(int id)
    {
        var inventory = await _context.Inventories
            .Include(item => item.Status)
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (inventory is null)
            return NotFound();

        if (!CanManage(inventory))
            return Forbid();

        if (inventory.Status?.Name is not ("Otvorena" or "U tijeku"))
            return BadRequest("Inventura mora biti otvorena ili u tijeku da bi se završila.");

        if (inventory.Items.Any(item => item.IsFound is null))
            return BadRequest("Sve stavke moraju biti obrađene prije završetka inventure.");

        inventory.StatusId = await GetInventoryStatusIdAsync("Završena");
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Završena -> Zaključana: konačno stanje, nakon ovoga se ništa ne smije mijenjati
    [HttpPost("{id}/lock")]
    //[Authorize(Policy = ClassAuthorizationPolicies.ViewEquipment)]
    public async Task<IActionResult> LockInventory(int id)
    {
        var inventory = await _context.Inventories
            .Include(item => item.Status)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (inventory is null)
            return NotFound();

        if (!CanManage(inventory))
            return Forbid();

        if (inventory.Status?.Name != "Završena")
            return BadRequest("Inventura mora biti završena da bi se zaključala.");

        inventory.StatusId = await GetInventoryStatusIdAsync("Zaključana");
        inventory.ClosedAt = DateTime.UtcNow;
        inventory.ClosedByUserId = CurrentUserId;

        await _context.SaveChangesAsync();
        return NoContent();
    }


    [HttpGet("{id}/items")]
    //[Authorize(Policy = ClassAuthorizationPolicies.ViewEquipment)]
    public async Task<ActionResult<List<InventoryItemDto>>> GetInventoryItems(int id, [FromQuery] InventoryItemFilterDto filter)
    {
        var query = _context.InventoryItems
            .Include(item => item.Equipment)
            .Include(item => item.ExpectedLocation)
            .Include(item => item.FoundLocation)
            .Include(item => item.ProcessedByUser)
            .Where(item => item.InventoryId == id)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var term = filter.SearchText.Trim().ToLower();
            query = query.Where(item =>
                item.ExpectedInventoryNumber.ToLower().Contains(term) ||
                item.Equipment!.Name.ToLower().Contains(term));
        }

        if (filter.IsFound.HasValue)
            query = query.Where(item => item.IsFound == filter.IsFound.Value);

        if (filter.IsDamaged.HasValue)
            query = query.Where(item => item.IsDamaged == filter.IsDamaged.Value);

        if (filter.LocationId.HasValue)
            query = query.Where(item => item.ExpectedLocationId == filter.LocationId.Value);

        var items = await query
            .OrderBy(item => item.ExpectedInventoryNumber)
            .Select(item => new InventoryItemDto
            {
                Id = item.Id,
                InventoryId = item.InventoryId,
                EquipmentId = item.EquipmentId,
                Equipment = item.Equipment!.Name,
                ExpectedInventoryNumber = item.ExpectedInventoryNumber,
                ExpectedLocationId = item.ExpectedLocationId,
                ExpectedLocation = item.ExpectedLocation!.Name,
                IsFound = item.IsFound,
                FoundLocationId = item.FoundLocationId,
                FoundLocation = item.FoundLocation != null ? item.FoundLocation.Name : null,
                IsDamaged = item.IsDamaged,
                DamageNote = item.DamageNote,
                ProcessedAt = item.ProcessedAt,
                ProcessedBy = item.ProcessedByUser != null ? item.ProcessedByUser.Username : null
            })
            .ToListAsync();

        return Ok(items);
    }


    [HttpPut("{id}/items/{itemId}")]
    //[Authorize(Policy = ClassAuthorizationPolicies.ViewEquipment)]
    public async Task<IActionResult> UpdateInventoryItem(int id, int itemId, SaveInventoryItemDto dto)
    {
        var inventory = await _context.Inventories
            .Include(item => item.Status)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (inventory is null)
            return NotFound();

        if (!CanManage(inventory))
            return Forbid();

        if (inventory.Status?.Name == "Zaključana")
            return BadRequest("Zaključana inventura se više ne može mijenjati.");

        if (inventory.Status?.Name is not ("Otvorena" or "U tijeku"))
            return BadRequest("Stavke se mogu obrađivati samo dok je inventura otvorena ili u tijeku.");

        var inventoryItem = await _context.InventoryItems.FirstOrDefaultAsync(item => item.Id == itemId && item.InventoryId == id);
        if (inventoryItem is null)
            return NotFound();

        inventoryItem.IsFound = dto.IsFound;
        inventoryItem.FoundLocationId = dto.FoundLocationId;
        inventoryItem.IsDamaged = dto.IsDamaged;
        inventoryItem.DamageNote = dto.DamageNote;
        inventoryItem.ProcessedAt = DateTime.UtcNow;
        inventoryItem.ProcessedByUserId = CurrentUserId;


        if (inventory.Status?.Name == "Otvorena")
            inventory.StatusId = await GetInventoryStatusIdAsync("U tijeku");

        await _context.SaveChangesAsync();
        return NoContent();
    }

    private bool CanManage(Inventory inventory)
    {
        var restriction = GetLocationRestriction();
        return !restriction.HasValue || inventory.LocationId == restriction.Value;
    }

    private async Task<int> GetInventoryStatusIdAsync(string name)
    {
        var status = await _context.InventoryStatuses.FirstOrDefaultAsync(item => item.Name == name);
        return status?.Id ?? throw new InvalidOperationException($"InventoryStatus '{name}' nije seed-an u bazi.");
    }

    private static InventoryDto ToDto(Inventory inventory) => new()
    {
        Id = inventory.Id,
        LocationId = inventory.LocationId,
        Location = inventory.Location?.Name ?? string.Empty,
        ResponsibleEmployeeId = inventory.ResponsibleEmployeeId,
        ResponsibleEmployee = inventory.ResponsibleEmployee != null
            ? inventory.ResponsibleEmployee.FirstName + " " + inventory.ResponsibleEmployee.LastName
            : string.Empty,
        StatusId = inventory.StatusId,
        Status = inventory.Status?.Name ?? string.Empty,
        OpenedAt = inventory.OpenedAt,
        ClosedAt = inventory.ClosedAt,
        Note = inventory.Note,
        TotalItemsCount = inventory.Items.Count,
        ProcessedItemsCount = inventory.Items.Count(item => item.IsFound != null),
        FoundCount = inventory.Items.Count(item => item.IsFound == true),
        MissingCount = inventory.Items.Count(item => item.IsFound == false),
        DamagedCount = inventory.Items.Count(item => item.IsDamaged == true)
    };
}
